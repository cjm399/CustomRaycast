inputfile = open('city.dae')
outputfile = open('vc_city.dae', 'w')

fullFile = ""
r = g = b = a = 0

for line in inputfile:
    if line.__contains__("<geometry"):
        cols = [r/255, g/255, b/255, a/255]
        a+=1
        if a > 255:
            b+=1
            a = 0
        if b > 255:
            g+=1
            b = 0
    if line.__contains__("colors-Cd-array") :
        index = line.index("\">")
        if line.__contains__("</float_array>"):
            endIndex = line.index("</float_array>")
            newLine = line[0 : index+2 : 1]
            colIndex = 0
            first = True
            for word in line[index+2 : endIndex : 1] :
                if colIndex > 3:
                    colIndex = 0
                if first:
                    newLine += str(cols[colIndex])
                    first = False
                else:
                    newLine += " " + str(cols[colIndex])
                colIndex += 1
            newLine += line[endIndex:len(line)]
            line = newLine
    fullFile += line
outputfile.write(fullFile)
outputfile.close()