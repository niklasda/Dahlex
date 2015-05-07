//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Dahlex.Logic.Settings;
//using Microsoft.WindowsAzure.MobileServices;

//namespace Dahlex.Azure
//{
//    public class LeaderboardService
//    {
//        //private const string AzureApplicationKey = "WTdmJlrTRyDUWPyUKqfjHLRXRhznnA55";
//        //private const string AzureServiceUri = "https://dahlmanlabs-leaderboards.azure-mobile.net/";

//        public async Task<List<HighScore>> GetLeaderboard()
//        {
//            var client = new MobileServiceClient(Config.AzureServiceUri, Config.AzureApplicationKey);
//            IMobileServiceTable<HighScore> hs = client.GetTable<HighScore>();

            
//            List<HighScore> b = await hs.ToListAsync();

//            return b;
//        }

//        public void SaveHighScore(HighScore score)
//        {
//            var client = new MobileServiceClient(Config.AzureServiceUri, Config.AzureApplicationKey);
//            IMobileServiceTable<HighScore> hs = client.GetTable<HighScore>();

//            // var score = new HighScore { Name = "Niklas", Score = 1, Game = 1 };

//            hs.InsertAsync(score);
//        }
//    }
//}