using System.Data;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using CloudNFT.Extensions;
using CloudNFT.Models;

namespace CloudNFT
{
    public class NFTGenerator
    {

        public List<NFTAttribute> Attributes = new List<NFTAttribute>();

        // AttributeId, AttributeId
        public Dictionary<int, List<int>> ItemExclusions = new Dictionary<int, List<int>>();
        public List<int> StormAlternates = new List<int>();
        private readonly string OutputRoot;
        public readonly string NFTImageRoot;
        public NFTGenerator()
        {
            Attributes.AddRange(GetCloudAttributes());
            Attributes.AddRange(GetHeadgearAttributes());
            Attributes.AddRange(GetEyebrowAttributes());
            Attributes.AddRange(GetEyewearAttributes());
            Attributes.AddRange(GetNeckAttributes());
            Attributes.AddRange(GetFaceAttributes());

            NFTImageRoot = @"C:\rootpathofyourimages";
            OutputRoot = @"C:\nftjsonoutput";
            InitExceptions();
        }

        private void InitExceptions()
        {
            var beards = Enumerable.Range(405, 9).Union(new List<int> { 417, 418 }).ToList();
            var bowties = new List<int> { 414, 415 };
            // No piercing with beards
            ItemExclusions.Add(502, beards);
            // No mustache with beards                        
            ItemExclusions.Add(503, beards);
            // No Blunt with beards or bowties
            ItemExclusions.Add(504, beards.Union(bowties).ToList());

            var bandanas = Enumerable.Range(303, 7).ToList();
            // No Mad Hatter with Bandanas
            ItemExclusions.Add(116, bandanas);

            // No Evil Eyebrows with tophat or bandanas or Mad Hatter or Baseball Cap
            ItemExclusions.Add(201, bandanas.Union(new List<int> { 101, 113, 116 }).ToList());

            StormAlternates = beards;
        }
        public void Run()
        {
            var uniqueNFTs = GenerateNFTs();
            CreateNFTs(uniqueNFTs.ToList().Randomize());
        }

        public IEnumerable<NFT> GenerateNFTs()
        {
            var uniques = Attributes.GroupBy(x => x.Category, x => x.Id).CartesianProduct();
            Console.WriteLine($"Unique NFTs: {uniques.Count()}");
            var uniquesSansExclusions = uniques.Where(x => HasNoExclusions(x)).ToList();
            Console.WriteLine($"Unique NFTs Sans Exclusions: {uniquesSansExclusions.Count()}");
            var uniqueNFTs = uniquesSansExclusions.Select(x => new NFT(x.SelectMany(y => Attributes.Where(z => z.Id == y))));
            return uniqueNFTs;
        }

        private void CreateNFTs(IEnumerable<NFT> nfts)
        {
            var magickPath = GetMagickPath();
            int nftNumber = 1;
            // Move favorite as first
            var favNFT = nfts.Where(x => x.Select(y => y.Id).Contains(600) && // Original
                                            x.Select(y => y.Id).Contains(116) && // Mad hatter
                                            x.Select(y => y.Id).Contains(302) && // 3D Glasses
                                            x.Select(y => y.Id).Contains(416) && // Rain
                                            x.Select(y => y.Id).Contains(501) && // Buckteeth
                                            x.Select(y => y.Id).Contains(200)).FirstOrDefault(); // No Eyebrows   
            nfts = new List<NFT> { favNFT }.Union(nfts);
            foreach (var nft in nfts)
            {
                var isStorm = nft.Select(x => x.Id).Contains(601);
                var layers = string.Join(" ", nft.OrderBy(x => (int)x.Category)
                                    .Select(x => string.Concat('"', (isStorm && StormAlternates.Contains(x.Id) ?
                                                                        x.ImageLayerPath.Replace("Neck/", "Neck/Storm-") :
                                                                        x.ImageLayerPath), '"')).ToArray());
                var outputName = $"{string.Join("-", nft.OrderBy(x => (int)x.Category).Select(x => x.Id).ToArray())}.png";
                var outputPath = Path.Combine(OutputRoot, $"{(isStorm ? "Storm" : "Cloud")}", outputName);
                var jsonPath = Path.Combine(OutputRoot, "ToProcess", $"{nftNumber}.json");
                nft.Id = nftNumber;
                nft.OutputFilePath = outputPath;

                if (!File.Exists(Path.Combine(NFTImageRoot, "Output", outputPath)))
                {
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = NFTImageRoot,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = magickPath,
                        Arguments = $"convert {layers} -background none -flatten {outputPath}",
                    };
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(nft, Newtonsoft.Json.Formatting.Indented));
                nftNumber++;
            }
            File.WriteAllLines(Path.Combine(OutputRoot, "all.csv"), nfts.Select(x => x.ToString()));
        }

        private string GetMagickPath()
        {
            var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");
            var paths = enviromentPath.Split(';');
            var exePath = paths.Select(x => Path.Combine(x, "magick.exe"))
                            .Where(x => File.Exists(x))
                            .FirstOrDefault();
            Console.WriteLine($"Found Magick Image Converter at: {exePath}");
            return exePath;
        }

        private bool HasNoExclusions(IEnumerable<int> list)
        {
            foreach (var x in list)
            {
                if (ItemExclusions.ContainsKey(x))
                {
                    var exceptionList = ItemExclusions[x];
                    if (exceptionList.Intersect(list).Any())
                        return false;
                }
            }
            return true;
        }

        private List<NFTAttribute> GetHeadgearAttributes()
        {
            return new List<NFTAttribute> {
                new NFTAttribute(){ Id = 100, Category = NFTCategory.Headgear, Name = "None", ImageLayerPath="none.png" },
                new NFTAttribute(){ Id = 101, Category = NFTCategory.Headgear, Name = "Top Hat", ImageLayerPath="Headgear/Top Hat.png" },
                new NFTAttribute(){ Id = 102, Category = NFTCategory.Headgear, Name = "Mohawk", ImageLayerPath="Headgear/Mohawk.png" },
                new NFTAttribute(){ Id = 103, Category = NFTCategory.Headgear, Name = "Lightning Mohawk", ImageLayerPath="Headgear/Lightning Mohawk.png" },
                new NFTAttribute(){ Id = 104, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Green", ImageLayerPath="Headgear/Lightning Mohawk Green.png" },
                new NFTAttribute(){ Id = 105, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Blue", ImageLayerPath="Headgear/Lightning Mohawk Blue.png" },
                new NFTAttribute(){ Id = 106, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Orange", ImageLayerPath="Headgear/Lightning Mohawk Orange.png" },
                new NFTAttribute(){ Id = 107, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Red", ImageLayerPath="Headgear/Lightning Mohawk Red.png" },
                new NFTAttribute(){ Id = 108, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Yellow", ImageLayerPath="Headgear/Lightning Mohawk Yellow.png" },
                new NFTAttribute(){ Id = 109, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Pink", ImageLayerPath="Headgear/Lightning Mohawk Pink.png" },
                new NFTAttribute(){ Id = 110, Category = NFTCategory.Headgear, Name = "Lightning Mohawk Purple", ImageLayerPath="Headgear/Lightning Mohawk Purple.png" },
                new NFTAttribute(){ Id = 111, Category = NFTCategory.Headgear, Name = "Wizard Hat", ImageLayerPath="Headgear/Wizard Hat.png" },
                new NFTAttribute(){ Id = 112, Category = NFTCategory.Headgear, Name = "Nerd Hat", ImageLayerPath="Headgear/Nerd Hat.png" },
                new NFTAttribute(){ Id = 113, Category = NFTCategory.Headgear, Name = "Baseball Cap", ImageLayerPath="Headgear/Baseball Cap.png" },
                new NFTAttribute(){ Id = 114, Category = NFTCategory.Headgear, Name = "Santa Hat", ImageLayerPath="Headgear/Santa Hat.png" },
                new NFTAttribute(){ Id = 115, Category = NFTCategory.Headgear, Name = "Bunny Ears", ImageLayerPath="Headgear/Bunny Ears.png" },
                new NFTAttribute(){ Id = 116, Category = NFTCategory.Headgear, Name = "Mad Hatter 10/6", ImageLayerPath="Headgear/Mad Hatter.png" },
            };
        }

        private List<NFTAttribute> GetEyebrowAttributes()
        {
            return new List<NFTAttribute> {
                new NFTAttribute(){ Id = 200, Category = NFTCategory.Eyebrows, Name = "None", ImageLayerPath="none.png" },
                new NFTAttribute(){ Id = 201, Category = NFTCategory.Eyebrows, Name = "Evil Eyebrows", ImageLayerPath="Eyebrows/Evil Eyebrows.png" },
            };
        }

        private List<NFTAttribute> GetEyewearAttributes()
        {
            return new List<NFTAttribute> {
                new NFTAttribute(){ Id = 300, Category = NFTCategory.Eyewear, Name = "None", ImageLayerPath="none.png" },
                new NFTAttribute(){ Id = 301, Category = NFTCategory.Eyewear, Name = "Square Glasses", ImageLayerPath="Eyewear/Square Glasses.png" },
                new NFTAttribute(){ Id = 302, Category = NFTCategory.Eyewear, Name = "3D Glasses", ImageLayerPath="Eyewear/3D Glasses.png" },
                new NFTAttribute(){ Id = 303, Category = NFTCategory.Eyewear, Name = "Bandana", ImageLayerPath="Eyewear/Bandana.png" },
                new NFTAttribute(){ Id = 304, Category = NFTCategory.Eyewear, Name = "Bandana Red", ImageLayerPath="Eyewear/Bandana Red.png" },
                new NFTAttribute(){ Id = 305, Category = NFTCategory.Eyewear, Name = "Bandana Green", ImageLayerPath="Eyewear/Bandana Green.png" },
                new NFTAttribute(){ Id = 306, Category = NFTCategory.Eyewear, Name = "Bandana Blue", ImageLayerPath="Eyewear/Bandana Blue.png" },
                new NFTAttribute(){ Id = 307, Category = NFTCategory.Eyewear, Name = "Bandana Purple", ImageLayerPath="Eyewear/Bandana Purple.png" },
                new NFTAttribute(){ Id = 308, Category = NFTCategory.Eyewear, Name = "Bandana Pink", ImageLayerPath="Eyewear/Bandana Pink.png" },
                new NFTAttribute(){ Id = 309, Category = NFTCategory.Eyewear, Name = "Bandana Yellow", ImageLayerPath="Eyewear/Bandana Yellow.png" },
                new NFTAttribute(){ Id = 310, Category = NFTCategory.Eyewear, Name = "Bloodshot Eyes", ImageLayerPath="Eyewear/Bloodshot.png" },
            };
        }

        private List<NFTAttribute> GetNeckAttributes()
        {
            return new List<NFTAttribute> {
                new NFTAttribute(){ Id = 400, Category = NFTCategory.Neck, Name = "None", ImageLayerPath="none.png" },
                new NFTAttribute(){ Id = 401, Category = NFTCategory.Neck, Name = "Lightning", ImageLayerPath="Neck/Lightning.png" },
                new NFTAttribute(){ Id = 402, Category = NFTCategory.Neck, Name = "Bitcoin Medallion", ImageLayerPath="Neck/Bitcoin.png" },
                new NFTAttribute(){ Id = 403, Category = NFTCategory.Neck, Name = "Ethereum Medallion", ImageLayerPath="Neck/Ethereum.png" },
                new NFTAttribute(){ Id = 404, Category = NFTCategory.Neck, Name = "Polygon Medallion", ImageLayerPath="Neck/Polygon.png" },
                new NFTAttribute(){ Id = 405, Category = NFTCategory.Neck, Name = "Beard", ImageLayerPath="Neck/Beard.png" },
                new NFTAttribute(){ Id = 406, Category = NFTCategory.Neck, Name = "Lightning Beard", ImageLayerPath="Neck/Lightning Beard.png" },
                new NFTAttribute(){ Id = 407, Category = NFTCategory.Neck, Name = "Lightning Beard Green", ImageLayerPath="Neck/Lightning Beard Green.png" },
                new NFTAttribute(){ Id = 408, Category = NFTCategory.Neck, Name = "Lightning Beard Blue", ImageLayerPath="Neck/Lightning Beard Blue.png" },
                new NFTAttribute(){ Id = 409, Category = NFTCategory.Neck, Name = "Lightning Beard Orange", ImageLayerPath="Neck/Lightning Beard Orange.png" },
                new NFTAttribute(){ Id = 410, Category = NFTCategory.Neck, Name = "Lightning Beard Red", ImageLayerPath="Neck/Lightning Beard Red.png" },
                new NFTAttribute(){ Id = 411, Category = NFTCategory.Neck, Name = "Lightning Beard Yellow", ImageLayerPath="Neck/Lightning Beard Yellow.png" },
                new NFTAttribute(){ Id = 412, Category = NFTCategory.Neck, Name = "Lightning Beard Pink", ImageLayerPath="Neck/Lightning Beard Pink.png" },
                new NFTAttribute(){ Id = 413, Category = NFTCategory.Neck, Name = "Lightning Beard Purple", ImageLayerPath="Neck/Lightning Beard Purple.png" },
                new NFTAttribute(){ Id = 414, Category = NFTCategory.Neck, Name = "Bowtie", ImageLayerPath="Neck/Bowtie.png" },
                new NFTAttribute(){ Id = 415, Category = NFTCategory.Neck, Name = "Bowtie Red" , ImageLayerPath="Neck/Bowtie Red.png"},
                new NFTAttribute(){ Id = 416, Category = NFTCategory.Neck, Name = "Rain", ImageLayerPath="Neck/Rain.png" },
                new NFTAttribute(){ Id = 417, Category = NFTCategory.Neck, Name = "Lightning Beard Rainbow", ImageLayerPath="Neck/Lightning Beard Rainbow.png" },

            };
        }

        private List<NFTAttribute> GetFaceAttributes()
        {
            return new List<NFTAttribute> {
                new NFTAttribute(){ Id = 500, Category = NFTCategory.Mouth, Name = "None", ImageLayerPath="none.png" },
                new NFTAttribute(){ Id = 501, Category = NFTCategory.Mouth, Name = "Buckteeth", ImageLayerPath="Mouth/Buckteeth.png" },
                new NFTAttribute(){ Id = 502, Category = NFTCategory.Mouth, Name = "Tongue Piercing", ImageLayerPath="Mouth/Piercing.png" },
                new NFTAttribute(){ Id = 503, Category = NFTCategory.Mouth, Name = "Mustache", ImageLayerPath="Mouth/Mustache.png" },
                new NFTAttribute(){ Id = 504, Category = NFTCategory.Mouth, Name = "Blunt", ImageLayerPath="Mouth/Blunt.png" },
            };
        }

        private List<NFTAttribute> GetCloudAttributes()
        {
            return new List<NFTAttribute> {
                new NFTAttribute(){ Id = 600, Category = NFTCategory.CloudType, Name = "Original", ImageLayerPath="Cloud.png"  },
                new NFTAttribute(){ Id = 601, Category = NFTCategory.CloudType, Name = "Storm",ImageLayerPath="StormCloud.png"  },
            };
        }

    }
}