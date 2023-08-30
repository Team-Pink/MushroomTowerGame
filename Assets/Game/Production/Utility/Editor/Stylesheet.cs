namespace EditorScripts
{
    using UnityEngine;

    public static class Stylesheet
    {
        // Label Field Styles
        public static GUIStyle TitleLabel
        {
            get => new(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, -5, -5)
            };
        }
        public static GUIStyle HeadingLabel
        {
            get => new(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
        }
        public static GUIStyle NoteLabel
        {
            get => new(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
        }
        public static GUIStyle RightLabel
        {
            get => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
            };
        }

        // Text Field Styles
        public static GUIStyle TitleTextField
        {
            get => new(GUI.skin.textField)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 5, 5)
            };
        }
        public static GUIStyle CentreText
        {
            get => new(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}