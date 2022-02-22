using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp.Serialization.JsonNet;
using LofiHollow.DataTypes;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LofiHollow.Managers {
    public class FirebaseManager {
        IFirebaseClient client;
        
        
        public FirebaseManager() {
            IFirebaseConfig config = new FirebaseConfig {
                AuthSecret = Properties.Resources.firebase,
                BasePath = "https://lofihollow-7b793-default-rtdb.firebaseio.com/"
            };

            client = new FirebaseClient(config);
        }

        public async Task Push(Feedback feed) {
            PushResponse result = await client.PushAsync("feedback", feed);

            if (result.StatusCode == System.Net.HttpStatusCode.OK) {
                GameLoop.UIManager.AddMsg("Feedback received. Thanks!");
            } else { 
                GameLoop.UIManager.AddMsg("Feedback Error: " + result.StatusCode + ". Sorry!");
            } 
        }
    }
}
