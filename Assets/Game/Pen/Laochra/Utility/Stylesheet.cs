#if UNITY_EDITOR
namespace Editor
{
    using UnityEngine;

    public static class Stylesheet
    {
        private static bool initialised;

        // Field Styles
        private static GUIStyle titleStyle = null;
        public static GUIStyle Title
        {
            get
            {
                if (!initialised)
                    Initialise();

                return titleStyle;
            }
            private set
            {
                titleStyle = value;
            }
        }
        private static GUIStyle headingStyle = null;
        public static GUIStyle Heading
        {
            get
            {
                if (!initialised)
                    Initialise();

                return headingStyle;
            }
            private set
            {
                headingStyle = value;
            }
        }
        private static GUIStyle noteStyle = null;
        public static GUIStyle Note
        {
            get
            {
                if (!initialised)
                    Initialise();

                return noteStyle;
            }
            private set
            {
                noteStyle = value;
            }
        }

        private static void Initialise()
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            noteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
        }
    }
}
#endif