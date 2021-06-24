using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;

public class InitializePropertyData : MonoBehaviour
{

    [SerializeField] private string propertyDataFilename = default;

    [HideInInspector] public Dictionary<string, property_data> placeIdToPropertyData = new Dictionary<string, property_data>();
    [HideInInspector] public List<string> listOfAddressesKnown = new List<string>();
    [HideInInspector] public List<uint> listOfKnownPlaceIds = new List<uint>();

    public delegate void NoArgs();
    public event NoArgs OnDataFinishedReading;

    private const string googleSpreadsheetBase = "https://docs.google.com/spreadsheets/d/";
    private const string googleExportFormat = "/export?format=csv";

    void Start()
    {
#if GOOGLE_HOSTED_DATA
        StartCoroutine(GoogleGetCSVData($"{googleSpreadsheetBase}1PX2BnQaZ859uLLAFuVWkjX9uSy225kflXMOIvOdcfng{googleExportFormat}"));
#else
        FileGetCSVData(propertyDataFilename);
#endif
    }

    private IEnumerator GoogleGetCSVData(string _dataURL)
    {
        string downloadedData = null;
        UnityWebRequest webRequest = UnityWebRequest.Get(_dataURL);

        yield return webRequest.SendWebRequest();

        if(webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError($"Error downloading building data from Google: {webRequest.error}");
        }
        else
        {
            downloadedData = webRequest.downloadHandler.text;
            ParseCSV(downloadedData);
        }
    }

    private void FileGetCSVData(string Filename)
    {
        string fileName = Path.Combine(Application.streamingAssetsPath, Filename);

        if(!File.Exists(fileName))
        {
            Debug.LogError("FILE COULD NOT BE FOUND!");
            return;
        }

        string csvData = File.ReadAllText(fileName);

        ParseCSV(csvData);
        FindObjectOfType<TestLoadCollada>().Initialize();
    }

    private void ParseCSV(string _csvData, int _startingLine = 1)
    {
        string[] lines = _csvData.Split(new string[] { System.Environment.NewLine, "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        Debug.Assert(lines.Length > 0, "No lines could be found in the parsed data.");
        Debug.Assert(_startingLine < lines.Length, "Starting line cannot be larger than the total number of lines in the csv.");
        if (_startingLine < 1) _startingLine = 1;

        string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
        Regex CSVParser = new Regex(pattern);


        int placeIdI = -1, categoryCodeI = -1, addressI = -1, marketValueI = -1,
            numberCondosI = -1, numberOfBedroomsI = -1, numberStoriesI = -1, taxableBuildingI = -1,
            taxableLandI = -1, totalAreaI = -1, totalLivableAreaI = -1, zoningI = -1, latI = -1, lngI = -1;

        string line0 = lines[0];
        string[] fields0 = CSVParser.Split(line0);
        for (int i = 0; i < fields0.Length; ++i)
        {
            switch (fields0[i])
            {
                case "placeId":
                    placeIdI = i;
                    break;
                case "category_code":
                    categoryCodeI = i;
                    break;
                case "location":
                    addressI = i;
                    break;
                case "market_value":
                    marketValueI = i;
                    break;
                case "number_condos":
                    numberCondosI = i;
                    break;
                case "number_of_bedrooms":
                    numberOfBedroomsI = i;
                    break;
                case "number_stories":
                    numberStoriesI = i;
                    break;
                case "taxable_building":
                    taxableBuildingI = i;
                    break;
                case "taxable_land":
                    taxableLandI = i;
                    break;
                case "total_area":
                    totalAreaI = i;
                    break;
                case "total_livable_area":
                    totalLivableAreaI = i;
                    break;
                case "zoning":
                    zoningI = i;
                    break;
                case "lat":
                    latI = i;
                    break;
                case "lng":
                    lngI = i;
                    break;
            }
        }

        for (int lineIndex = _startingLine;
            lineIndex < lines.Length;
            ++lineIndex)
        {
            string line = lines[lineIndex];
            string[] fields = line.Split(',');

            uint placeId;
            property_data curr = new property_data();

            if (!uint.TryParse(fields[marketValueI], System.Globalization.NumberStyles.Float, null, out curr.marketValue))
            {
                Debug.LogError("Market Value was not parsed successfully\n" + fields[marketValueI]);
            }
            if (!byte.TryParse(fields[categoryCodeI], out curr.categoryCode))
            {
                Debug.LogError("Category Code was not parsed successfully\n" + fields[categoryCodeI]);
            }
            if (!uint.TryParse(fields[taxableBuildingI], System.Globalization.NumberStyles.Float, null, out curr.taxableBuilding))
            {
                Debug.LogError("Taxable Building was not parsed successfully\n" + fields[taxableBuildingI]);
            }
            if (!uint.TryParse(fields[taxableLandI], System.Globalization.NumberStyles.Float, null, out curr.taxableLand))
            {
                Debug.LogError("Taxable Land was not parsed successfully\n" + fields[taxableLandI]);
            }
            if (!double.TryParse(fields[latI], out curr.lat))
            {
                Debug.LogError("Lat was not parsed successfully\n" + fields[latI]);
            }
            if (!double.TryParse(fields[lngI], out curr.lng))
            {
                Debug.LogError("Lng was not parsed successfully\n" + fields[latI]);
            }
            if (!uint.TryParse(fields[placeIdI], System.Globalization.NumberStyles.Integer, null, out placeId))
            {
                Debug.LogError("PlaceId was not parsed successfully\n" + fields[placeIdI]);
            }
            if (!uint.TryParse(fields[totalAreaI], System.Globalization.NumberStyles.Integer, null, out curr.totalArea))
            {
                Debug.LogError("Total Area was not parsed successfully\n" + fields[totalAreaI]);
            }
            if (!uint.TryParse(fields[totalLivableAreaI], System.Globalization.NumberStyles.Integer, null, out curr.totalLivableArea))
            {
                Debug.LogError("Total Livable Area was not parsed successfully\n" + fields[totalLivableAreaI]);
            }
            if (!ushort.TryParse(fields[numberCondosI], System.Globalization.NumberStyles.Integer, null, out curr.numberCondos))
            {
                Debug.LogError("Number of Condos was not parsed successfully\n" + fields[numberCondosI]);
            }
            if (!ushort.TryParse(fields[numberStoriesI], System.Globalization.NumberStyles.Integer, null, out curr.numberOfStories))
            {
                Debug.LogError("Number of Stories was not parsed successfully\n" + fields[numberStoriesI]);
            }
            if (!ushort.TryParse(fields[numberOfBedroomsI], System.Globalization.NumberStyles.Integer, null, out curr.numberBedRooms))
            {
                Debug.LogError("Number of Bedrooms was not parsed successfully\n" + fields[numberOfBedroomsI]);
            }
            curr.placeID = placeId;
            curr.zone = PropertyData.GetZoningByte(fields[zoningI]);
            curr.address = fields[addressI];

            if (placeIdToPropertyData.ContainsKey(placeId.ToString()))
            {
                //Debug.LogError(placeId + " ALREADY EXISTS IN THE DICT!");
                property_data data = placeIdToPropertyData[placeId.ToString()];
                data.marketValue += curr.marketValue;
                data.taxableBuilding += curr.taxableBuilding;
                data.taxableLand += curr.taxableLand;
                placeIdToPropertyData[placeId.ToString()] = data;
            }
            else
            {
                listOfAddressesKnown.Add(curr.address);
                listOfKnownPlaceIds.Add(curr.placeID);
                placeIdToPropertyData.Add(curr.placeID.ToString(), curr);
            }
        }
        OnDataFinishedReading?.Invoke();
    }
}
