public enum Zones
{
    Comercial = 0,
    Industrial = 1,
    Residential = 2,
    SpecialPurpose = 3,
    Parks = 4,
    Destroyed = 5,
    All
}

public struct property_data
{
    public string address;
    public uint marketValue;
    public byte categoryCode;
    public uint taxableBuilding;
    public uint taxableLand;
    public byte zone;
    public double lat;
    public double lng;
    public ushort numberBedRooms;
    public ushort numberOfStories;
    public ushort numberCondos;
    public uint totalArea;
    public uint totalLivableArea;
    public uint placeID;
    public byte isFake;
    public bool isDestroyed => zone == (int)Zones.Destroyed;
    public bool isBuildingVisible => (zone != (int)Zones.Destroyed && zone != (int)Zones.Parks);
}

public static class PropertyData
{
    public static UnityEngine.Color32 pricingMaxColor = new UnityEngine.Color32(25, 75, 25, 255);
    public static UnityEngine.Color32 pricingMinColor = new UnityEngine.Color32(235, 255, 0, 255);

    public static string
        ToCSVLine(ref property_data _p)
    {
        return _p.placeID + ',' +
               _p.categoryCode + "," +
               _p.address + "," +
               _p.marketValue + "," +
               _p.numberCondos + "," +
               _p.numberBedRooms + "," +
               _p.numberOfStories + "," +
               _p.taxableBuilding + "," +
               _p.taxableLand + "," +
               _p.totalArea + "," +
               _p.totalLivableArea + "," +
               _p.zone + "," +
               _p.lat + "," +
               _p.lng;

    }

    public static UnityEngine.Color32
        GetPricingColor(uint _price)
    {
        if (_price < 950_000)
        {
            return pricingMinColor;
        }
        else if (_price < 1_500_000)
        {
            return new UnityEngine.Color32(127, 185, 5, 255);
        }
        else if (_price < 10_000_000)
        {
            return new UnityEngine.Color32(0, 134, 0, 255);
        }
        else
        {
            return pricingMaxColor;
        }
    }

    public static string[]
        catCodeStrings = new string[]
        {
            "Residential",
            "Hotels and Apartments",
            "Store with Dwelling",
            "Commercial",
            "Industrial",
            "Vacant Land"
        };

    public static string[]
        zoningGroups = new string[]
    {
            "Commercial/Commercial Mixed-Use",
            "Industrial/Industrial Mixed-Use",
            "Residentail /Residential Mixed-Use",
            "Special Purpose",
            "Park",
            "Destroyed",
            "All Zones"
    };

    public static UnityEngine.Color32[]
        zoningColors = new UnityEngine.Color32[]
    {
        new UnityEngine.Color32(151,174,248,255), //Blue Pocket City
        new UnityEngine.Color32(242, 197, 68, 255), //Yellow Pocket City
        new UnityEngine.Color32(167,208,91,255), //Green Pocket City
        new UnityEngine.Color32(173,152,212,255), //Purple Pastel
        new UnityEngine.Color32(72,236,125,255), //Lime Green Grass,
        new UnityEngine.Color32(255,0,0,255), //Bright Bad Red
    };

    public static UnityEngine.Color32[]
    zoningMappingColors = new UnityEngine.Color32[]
{
        new UnityEngine.Color32(0,0,1,0), //Blue Pocket City
        new UnityEngine.Color32(0,0,2,0), //Yellow Pocket City
        new UnityEngine.Color32(0,0,3,0), //Green Pocket City
        new UnityEngine.Color32(0,0,4,0), //Purple Pastel
        new UnityEngine.Color32(0,0,5,0), //Lime Green Grass,
        new UnityEngine.Color32(0,0,6,0), //Bright Bad Red
};


    public static string
        CategoryCode(byte catCode)
    {
        if (catCode > 5)
            return "ERROR : INVALID CAT CODE";

        return catCodeStrings[catCode];
    }

    public static string
        ZoningGroup(byte _zone)
    {
        if (_zone > zoningGroups.Length - 1)
            return "ERROR : INVALID ZONE";

        return zoningGroups[_zone];
    }

    public static byte
        GetZoningByte(string zone)
    {
        //string pattern = @"\d+$";
        //System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);
        string begining = System.Text.RegularExpressions.Regex.Replace(zone, @"(\d|\.|\s)", "");

        //NOTE: I don't know how Special Purpose buildings are sent in, so I don't know how it will come back from regex.

        switch (begining)
        {
            case "CA": //CA-1, CA-2, CMX-1, CMX-2, CMX-2.5, CMX-3, CMX-4, CMX-5
            case "CMX":
                //return "Commercial/Commercial Mixed-Use";
                return 0;

            case "I": //I-1, I-2, I-3, I-P, ICMX, IRMX, 12
            case "ICMX":
            case "IRMX":
            case "12":
                //return "Industrial/Industrial Mixed-Use";
                return 1;

            case "RM": //RM-1, RM-2, RM-3, RM-4, RMX-1, RMX-2, RMX-3, RSA-1, RSA-2, RSA-3, RSA-4, RSA-5, RSD-1, RSD-2, RSD-3, RTA-1
            case "RMX":
            case "RSA":
            case "RSD":
            case "RTA":
                //return "Residentail /Residential Mixed-Use";
                return 2;

            case "SP": //SP-AIR, SP-ENT, SP-PO-A, SP-INS, SP-PO-P, SP-STA
            case "SPPOA":
            
                //return "Special Purpose";
                return 3;
            case "": //Empty
                return 0;
            default:
                //return "ERROR : INVALID ZONE";
                UnityEngine.Debug.LogError($"Bad Zone Switch : {begining} in {zone}");
                return 255;
        }
    }
}