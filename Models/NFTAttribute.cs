using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;
using System.Linq;

namespace CloudNFT.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NFT : List<NFTAttribute>{

        [JsonProperty]
        public int Id {get;set;}
        
        [JsonProperty]
        public Dictionary<string,string> Attributes {get;set;}

        [JsonProperty]
        public int AttributeCount
        {
            get
            {
                return Attributes.AsEnumerable().Where(x=> x.Value != "None").Count();
            }
        }

        public NFT(){            
        }

        public NFT(IEnumerable<NFTAttribute> attributes){
            this.Clear();
            this.AddRange(attributes);
            Attributes = new Dictionary<string,string>();
            foreach(var attr in attributes)
            Attributes.Add(Enum.GetName(typeof(NFTCategory), attr.Category), attr.Name);
       }

       [JsonProperty]
        public string OutputFilePath {get;set;}

        public string JsonFilePath {get;set;}

        [JsonProperty]
        public string OSURL {get;set;}

        public override string ToString(){
            return $"{Id},{AttributeCount},\"{OSURL}\",\"{OutputFilePath}\",0,\"{Attributes["CloudType"]}\",\"{Attributes["Headgear"]}\",\"{Attributes["Eyebrows"]}\",\"{Attributes["Eyewear"]}\",\"{Attributes["Neck"]}\",\"{Attributes["Mouth"]}\"";
        }

    }
    public class NFTAttribute
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public NFTCategory Category { get; set; }
        public string ImageLayerPath { get; set; }

        public override string ToString(){
            return $"{Id}~{Name}~{Category}~{ImageLayerPath}";
        }
    }
}