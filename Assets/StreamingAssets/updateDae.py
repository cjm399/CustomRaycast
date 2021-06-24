inputfile = open('box.dae')
outputfile = open('city_updated.dae', 'w')

fullFile = ""
meshCount = -1
r = g = b = a = 0

for line in inputfile:
    if line.__contains__("<geometry"):
        meshCount +=1
        a+=1
        if a >= 255:
            b+=1
            a = 0
        if b >= 255:
            g+=1
            b = 0
    if line.__contains__("colors-Col-array") :
        index = line.index("\">")
        if line.__contains__("</float_array>"):
            endIndex = line.index("</float_array>")
            newLine = line[0 : index+2 : 1]
            colIndex = 0
            for word in line[index+2 : endIndex : 1] :
                colIndex+=1
                newLine += " " + str(meshCount)
            newLine += line[endIndex:len(line)]
            line = newLine
    fullFile += line
print(fullFile)