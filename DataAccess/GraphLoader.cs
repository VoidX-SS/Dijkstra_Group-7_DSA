using System;
using System.IO;
using System.Collections.Generic;

namespace DoAnCuoiKy_Dijkstra
{
    public static class GraphLoader
    {
        public static Graph LoadGraph(string locationPath, string edgePath)
        {
            if (!File.Exists(locationPath))
                throw new FileNotFoundException("Không tìm thấy file địa điểm tại: " + locationPath);
            if (!File.Exists(edgePath))
                throw new FileNotFoundException("Không tìm thấy file cạnh tại: " + edgePath);

            Graph graph = new Graph();

            // 1. Load Locations với StreamReader
            using (StreamReader reader = new StreamReader(locationPath))
            {
                if (!reader.EndOfStream)
                    reader.ReadLine(); // Bỏ qua dòng tiêu đề

                int lineNum = 1;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lineNum++;

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Phân tách chuỗi bằng IndexOf
                    int firstComma = line.IndexOf(',');
                    if (firstComma == -1) continue;

                    int secondComma = line.IndexOf(',', firstComma + 1);
                    if (secondComma == -1) continue;

                    int thirdComma = line.IndexOf(',', secondComma + 1);
                    if (thirdComma == -1) continue;

                    string id = line.Substring(0, firstComma).Trim();
                    string name = line.Substring(firstComma + 1, secondComma - firstComma - 1).Trim();
                    string xStr = line.Substring(secondComma + 1, thirdComma - secondComma - 1).Trim();
                    
                    int fourthComma = line.IndexOf(',', thirdComma + 1);
                    string yStr = fourthComma == -1 ? line.Substring(thirdComma + 1).Trim() : line.Substring(thirdComma + 1, fourthComma - thirdComma - 1).Trim();

                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                        throw new FormatException("ID hoặc Tên địa điểm tại dòng " + lineNum + " không được để trống.");

                    if (!double.TryParse(xStr, out double x))
                        throw new FormatException("Tọa độ X tại dòng " + lineNum + " không hợp lệ.");

                    if (!double.TryParse(yStr, out double y))
                        throw new FormatException("Tọa độ Y tại dòng " + lineNum + " không hợp lệ.");

                    try 
                    {
                        graph.AddVertex(id, name, x, y);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new InvalidDataException("Lỗi dữ liệu tại dòng " + lineNum + ": " + ex.Message);
                    }
                }
            }

            if (graph.VertexCount == 0)
                throw new InvalidDataException("File CSV không chứa địa điểm nào hợp lệ.");

            // 2. Load Edges
            using (StreamReader reader = new StreamReader(edgePath))
            {
                if (!reader.EndOfStream)
                    reader.ReadLine(); // Bỏ qua dòng tiêu đề

                int lineNum = 1;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    lineNum++;

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    int firstComma = line.IndexOf(',');
                    if (firstComma == -1) continue;

                    int secondComma = line.IndexOf(',', firstComma + 1);
                    if (secondComma == -1) continue;

                    string id1 = line.Substring(0, firstComma).Trim();
                    string id2 = line.Substring(firstComma + 1, secondComma - firstComma - 1).Trim();
                    string directedStr = line.Substring(secondComma + 1).Trim();

                    if (string.IsNullOrEmpty(id1) || string.IsNullOrEmpty(id2))
                        continue;

                    bool isDirected = directedStr == "1";

                    try
                    {
                        if (isDirected)
                            graph.AddDirectedEdge(id1, id2);
                        else
                            graph.AddUndirectedEdge(id1, id2);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return graph;
        }
    }
}
