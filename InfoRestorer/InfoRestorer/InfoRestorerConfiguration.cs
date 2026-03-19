using Rocket.API;

namespace InfoRestorer
{
    public class InfoRestorerConfiguration : IRocketPluginConfiguration
    {
        public string MessageColor { get; set; }
        public bool ShouldClearInventory { get; set; }
        public bool ShouldRemoveSavesOnLeave { get; set; }
        public int MaxSavesPerPlayer { get; set; }

        public void LoadDefaults()
        {
            MessageColor = "green";
            ShouldClearInventory = true;
            ShouldRemoveSavesOnLeave = false;
            MaxSavesPerPlayer = 5;
        }
    }
}
