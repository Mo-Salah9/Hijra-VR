using UnityEngine;

namespace CrizGames.Vour
{
    public class StreamingAssetsPathAttribute : PropertyAttribute
    {
        public string fileExtensionFilter;

        public StreamingAssetsPathAttribute(string fileExtensionFilter = "")
        {
            this.fileExtensionFilter = fileExtensionFilter;
        }
    }
}