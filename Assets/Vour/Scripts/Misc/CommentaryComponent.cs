using System;
using UnityEngine;

// Adapted from https://github.com/Deadcows/MyBox/blob/master/Types/CommentaryComponent.cs
namespace CrizGames.Vour
{
	public class CommentaryComponent : MonoBehaviour
	{
#if UNITY_EDITOR
		[Serializable]
		public struct Entry
		{
			public string EditorCommentary;
			public UnityEditor.MessageType Type;
		}

		public Entry[] Entries;
#endif
	}
}

#if UNITY_EDITOR
namespace CrizGames.Vour.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(CommentaryComponent))]
	public class CommentaryDrawer : Editor
	{
		private CommentaryComponent _commentary;
		private GUIContent _boxContent;

		private bool _editMode;

		public override void OnInspectorGUI()
		{
			if (_commentary == null) _commentary = (CommentaryComponent)target;
			if (_commentary.Entries == null)
			{
				_commentary.Entries = Array.Empty<CommentaryComponent.Entry>();
				EditorUtility.SetDirty(_commentary);
			}

			var displayMode = !_editMode && _commentary.Entries.Length > 0;
			if (displayMode) DrawCommentariesDisplayMode();
			else DrawCommentariesEditMode();
		}

		private void DrawCommentariesDisplayMode()
		{
			var e = Event.current;
			var mousePosition = e.mousePosition;
			var isClick = e.button == 0 && e.isMouse;

			bool firstEntry = true;
			foreach (var entry in _commentary.Entries)
			{
				if (!firstEntry) EditorGUILayout.Space(2);
				firstEntry = false;
				EditorGUILayout.HelpBox(entry.EditorCommentary, entry.Type);
				if (GUILayoutUtility.GetLastRect().Contains(mousePosition) && isClick) _editMode = true;
			}
		}
		
		private void DrawCommentariesEditMode()
		{
			for (var i = 0; i < _commentary.Entries.Length; i++)
			{
				var entry = _commentary.Entries[i];
				using (new EditorGUILayout.HorizontalScope())
				{
					using (new GUILayout.VerticalScope(GUILayout.Width(40)))
					{
						GUILayout.Space(4);
						if (GUILayout.Button(GetIcon(entry.Type), EditorStyles.helpBox, GUILayout.Width(40), GUILayout.Height(36)))
						{
							_commentary.Entries[i].Type = NextType(entry.Type);
						}

						if (GUILayout.Button("×", GUILayout.Width(40)))
						{
							var index = i;
							EditorApplication.delayCall += () =>
							{
								_commentary.Entries = RemoveAt(_commentary.Entries, index);
								EditorUtility.SetDirty(_commentary);
								Repaint();
							};
						}
					}

					_commentary.Entries[i].EditorCommentary = EditorGUILayout.TextArea(entry.EditorCommentary, EditorStyles.helpBox);
				}
			}

			EditorGUILayout.Space();
			using (new GUILayout.HorizontalScope())
			{
				if (_commentary.Entries.Length > 0 && GUILayout.Button("✓", GUILayout.Width(40))) _editMode = false;
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("+", GUILayout.Width(40)))
				{
					Array.Resize(ref _commentary.Entries, _commentary.Entries.Length + 1);
					_editMode = true;
					EditorUtility.SetDirty(_commentary);
				}
			}


			if (GUI.changed) EditorUtility.SetDirty(target);
		}

		private GUIContent GetIcon(MessageType type)
		{
			if (type == MessageType.Info) return EditorGUIUtility.IconContent("console.infoicon");
			if (type == MessageType.Warning) return EditorGUIUtility.IconContent("console.warnicon");
			if (type == MessageType.Error) return EditorGUIUtility.IconContent("console.erroricon");
			return new GUIContent("No icon");
		}

		private MessageType NextType(MessageType type)
		{
			if (type == MessageType.Info) return MessageType.Warning;
			if (type == MessageType.Warning) return MessageType.Error;
			if (type == MessageType.Error) return MessageType.None;
			return MessageType.Info;
		}
		
		/// <summary>
		/// Returns new array without element at index
		/// </summary>
		private static T[] RemoveAt<T>(T[] array, int index)
		{
			if (index < 0)
			{
				Debug.LogError("Index is less than zero. Array is not modified");
				return array;
			}

			if (index >= array.Length)
			{
				Debug.LogError("Index exceeds array length. Array is not modified");
				return array;
			}

			T[] newArray = new T[array.Length - 1];
			int index1 = 0;
			for (int index2 = 0; index2 < array.Length; ++index2)
			{
				if (index2 == index) continue;

				newArray[index1] = array[index2];
				++index1;
			}

			return newArray;
		}
	}
}
#endif