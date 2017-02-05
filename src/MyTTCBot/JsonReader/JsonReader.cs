using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyTTCBot.Models;
namespace MyTTCBot.JsonReader
{
    public static class JsonReader
    {
        public static Station LoadJson(String filepath)
        {
            try
            {
                using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string json = sr.ReadToEnd();
                        Station station = JsonConvert.DeserializeObject<Station>(json);
                        return station;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
