﻿using FPOSDB.DTO;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPOSPriceUpdater.Helper
{
    public static class Serializer
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void ExportDataToCsv(List<ItemPriceDTO> data, string path)
        {
                WriteHeadersToCsv(path);
                WriteRowsToCsv(data, path);
        }

        private static void WriteRowsToCsv(List<ItemPriceDTO> data, string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                foreach (ItemPriceDTO item in data)
                {
                    if (!String.IsNullOrEmpty(item.ItemName) && !String.IsNullOrEmpty(item.ItemID))
                    {
                        var row = string.Join(";", typeof(ItemPriceDTO).GetProperties().Select(x => x.GetValue(item, null).ToString()).ToArray());
                        writer.WriteLine(row);
                    }
                }
            }
        }

        private static void WriteHeadersToCsv(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                string headers = string.Join(";", typeof(ItemPriceDTO).GetProperties().Select(x => x.Name).ToArray());
                writer.WriteLine(headers);
            }

        }

        public static void WriteToFile(List<ItemPriceDTO> data, string fileName)
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + fileName;
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                List<string> itemsDone = new List<string>();
                
                writer.WriteLine(DateTime.Now);
                foreach(var item in data)
                {
                    if (!itemsDone.Contains(item.ItemName))
                    {
                        writer.WriteLine(item);
                        itemsDone.Add(item.ItemName);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a CSV file to a list of objects
        /// </summary>
        /// <param name="path">the full path to the csv file</param>
        /// <returns></returns>
        public static List<ItemPriceDTO> FromCSV(string path)
        {
            List<ItemPriceDTO> items = new List<ItemPriceDTO>();
            using(StreamReader reader = new StreamReader(path))
            {
                ItemPriceDTO item;
                string[] headers = reader.ReadLine().Split(';');

                string line;
                while ((line = reader.ReadLine()) != null) {
                    string[] row = line.Split(';');
                    item = new ItemPriceDTO();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        try
                        {
                            string newValue = row[i];
                            item.GetType().GetProperty(headers[i]).SetValue(item, newValue, null);
                        } catch(Exception ex) { 
                            _log.Error("Converting CSV cell to object resulted in an error",ex);
                            throw ex;
                        }
                    }
                    items.Add(item);  
                }
            }
            return items;
        }
    }
}
