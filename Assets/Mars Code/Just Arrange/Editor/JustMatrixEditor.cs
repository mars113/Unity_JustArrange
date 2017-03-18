/**************************************************
* Description
* 
* 
**************************************************/

namespace MarsCode
{
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(JustMatrix))]
    public class JustMatrixEditor : Editor
    {

        #region >>  Fields & Properties

        JustMatrix obj;

        SerializedProperty width, height, columns, rows;

        SerializedProperty bend;

        SerializedProperty steps, ladder;

        Color[] colors = new Color[6];

        GUIStyle lableButtonStyle;

        bool stepsWholeNumber, ladderWholeNumber, wholeNumberZ;

        Vector3[] rowStartPos;

        #endregion


        #region >>  基本函式

        void OnEnable()
        {
            obj = target as JustMatrix;

            width = serializedObject.FindProperty("width");
            height = serializedObject.FindProperty("height");
            columns = serializedObject.FindProperty("columns");
            rows = serializedObject.FindProperty("rows");
            bend = serializedObject.FindProperty("bend");
            steps = serializedObject.FindProperty("steps");
            ladder = serializedObject.FindProperty("ladder");

            ColorUtility.TryParseHtmlString("#a4f4ff", out colors[0]);
            ColorUtility.TryParseHtmlString("#fff177", out colors[1]);
            ColorUtility.TryParseHtmlString("#a8ffbb", out colors[2]);
            ColorUtility.TryParseHtmlString("#d3a0ff", out colors[3]);
            ColorUtility.TryParseHtmlString("#f69640", out colors[4]);
            ColorUtility.TryParseHtmlString("#ffb3f8", out colors[5]);

            SyncPreviewPointPos();
        }


        public override void OnInspectorGUI()
        {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;
            GUI.skin.button.fontStyle = FontStyle.Bold;

            lableButtonStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };

            //程式運行時禁止操作
            if(Application.isPlaying && GUI.enabled)
            {
                GUI.enabled = false;
            }

            serializedObject.Update();

            if(!DrawBasicHandlers())
            {
                return;
            }

            DrawAdvencedHandlers();

            if(GUILayout.Button(new GUIContent("Generate")))
            {
                Generate();
            }

            if(GUILayout.Button(new GUIContent("Clear")))
            {
                Clear();
            }

            StateDetector();

            serializedObject.ApplyModifiedProperties();
        }


        void OnSceneGUI()
        {
            DrawPreviewHandlers();
        }


        /// <summary>
        /// 偵測物件與編輯器的狀態
        /// </summary>
        void StateDetector()
        {
            //偵測Inspector的變更狀態
            if(GUI.changed)
            {
                SyncPreviewPointPos();
            }

            //偵測transform的變更狀態
            if(obj.transform.hasChanged)
            {
                obj.transform.hasChanged = false;
                SyncPreviewPointPos();
            }
        }

        #endregion


        #region >>  Inspector UI

        /// <summary>
        /// 繪製基本控制項
        /// </summary>
        bool DrawBasicHandlers()
        {
            GUI.color = colors[0];
            EditorGUILayout.BeginHorizontal("Box");
            {
                GUI.color = Color.white;
                //Source
                GUILayout.Label(new GUIContent("Source"), lableButtonStyle, LableWidth());

                GameObject go = EditorGUILayout.ObjectField(obj.go, typeof(GameObject), true) as GameObject;
                if(obj.go != go)
                {
                    Undo.RecordObject(obj, "Assigned GameObject");
                    obj.go = go;
                }
            }
            EditorGUILayout.EndHorizontal();

            if(obj.go == null)
            {
                var fontSize = EditorStyles.helpBox.fontSize;
                EditorStyles.helpBox.fontSize = 14;
                EditorGUILayout.HelpBox("指派Source物件以進行操作", MessageType.Info);
                EditorStyles.helpBox.fontSize = fontSize;
                return false;
            }

            GUI.color = colors[1];
            EditorGUILayout.BeginVertical("Box");
            {
                GUI.color = Color.white;

                //widt
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent("Width"), lableButtonStyle, LableWidth()))
                    {
                        width.floatValue = 1;
                    }
                    width.floatValue = EditorGUILayout.Slider(width.floatValue, 1, 500);
                }
                EditorGUILayout.EndHorizontal();

                //height
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent("Height"), lableButtonStyle, LableWidth()))
                    {
                        height.floatValue = 1;
                    }
                    height.floatValue = EditorGUILayout.Slider(height.floatValue, 1, 500);
                }
                EditorGUILayout.EndHorizontal();

                //columns
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent("Columns"), lableButtonStyle, LableWidth()))
                    {
                        columns.intValue = 1;
                    }
                    columns.intValue = EditorGUILayout.IntSlider(columns.intValue, 1, 50);
                }
                EditorGUILayout.EndHorizontal();

                //rows
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent("Rows"), lableButtonStyle, LableWidth()))
                    {
                        rows.intValue = 1;
                    }
                    rows.intValue = EditorGUILayout.IntSlider(rows.intValue, 1, 50);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            return true;
        }


        /// <summary>
        /// 繪製進階控制項
        /// </summary>
        void DrawAdvencedHandlers()
        {
            GUI.color = colors[2];
            EditorGUILayout.BeginHorizontal("Box");
            {
                bool current = GUI.enabled;

                if(columns.intValue < 3)
                {
                    GUI.enabled = false;
                }

                GUI.color = Color.white;
                if(GUILayout.Button(new GUIContent("Bend"), lableButtonStyle, LableWidth()))
                {
                    bend.floatValue = 0;
                }

                bend.floatValue = EditorGUILayout.Slider(bend.floatValue, 0, 360f / columns.intValue);
                GUI.enabled = current;

            }
            EditorGUILayout.EndHorizontal();

            GUI.color = colors[3];
            EditorGUILayout.BeginVertical("Box");
            {
                GUI.color = Color.white;

                //OffsetX
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent("Steps"), lableButtonStyle, LableWidth()))
                    {
                        steps.floatValue = 0;
                    }
                    steps.floatValue = EditorGUILayout.Slider(steps.floatValue, -3, 3);
                    stepsWholeNumber = EditorGUILayout.Toggle(new GUIContent(""), stepsWholeNumber, GUILayout.Width(20));
                    if(stepsWholeNumber)
                    {
                        steps.floatValue = Mathf.Round(steps.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();

                //OffsetY
                EditorGUILayout.BeginHorizontal();
                {
                    if(GUILayout.Button(new GUIContent("Ladder"), lableButtonStyle, LableWidth()))
                    {
                        ladder.floatValue = 0;
                    }
                    ladder.floatValue = EditorGUILayout.Slider(ladder.floatValue, -3, 3);
                    ladderWholeNumber = EditorGUILayout.Toggle(new GUIContent(""), ladderWholeNumber, GUILayout.Width(20));
                    if(ladderWholeNumber)
                    {
                        ladder.floatValue = Mathf.Round(ladder.floatValue);
                    }
                }
                EditorGUILayout.EndHorizontal();

                var fontSize = EditorStyles.helpBox.fontSize;
                EditorStyles.helpBox.fontSize = 14;
                EditorGUILayout.HelpBox("勾選Toggle以將數值拘束為整數", MessageType.Info);
                EditorStyles.helpBox.fontSize = fontSize;
            }
            EditorGUILayout.EndVertical();
        }


        GUILayoutOption LableWidth()
        {
            return GUILayout.Width(70);
        }

        #endregion


        #region >>  主要功能

        /// <summary>
        /// 設置預覽陣列的座標點
        /// <para>矩陣物件的排列是由物體的Loacel座標右上方開始排列到Local左上方</para>
        /// <para>並且依照指定的列數往下排列</para>
        /// </summary>
        void SyncPreviewPointPos()
        {
            var tran = obj.transform;

            //bend必須要在行數在3以上才成立
            //未滿條件則重設bend的值為0
            if(columns.intValue < 3)
            {
                bend.floatValue = 0;
            }

            //在行數為奇數與偶數的情況下，處理的方式會有所差異
            //在此設置一個flag於後面方便標識
            bool onOddColumns = (columns.intValue % 2 == 1);

            //行數等於1，則不需要計算陣列起始的X座標
            //行數大於1則算出陣列的起始X座標與每一行彼此的水平間隔
            var col = columns.intValue;
            var spaceX = col == 1 ? 0 : width.floatValue / (col - 1);

            //列數等於1，則不需要計算陣列起始的Y座標
            //列數大於1則算出陣列的起始Y座標與每一列彼此的垂直間隔
            var row = rows.intValue;
            var startY = row == 1 ? 0 : height.floatValue * 0.5f;
            var spaceY = row == 1 ? 0 : height.floatValue / (row - 1);

            //依據指定的行列數調整position的尺寸
            obj.positions = new Vector3[col, row];

            //以物件位置X=0為出發的列高參考座標
            rowStartPos = new Vector3[row];

            for(int r = 0; r < row; r++)
            {
                //取得物件向前的方位 + 高度差 + 階梯差
                float posX = 0;
                float posY = startY - spaceY * r;
                float posZ = posY * -steps.floatValue;
                rowStartPos[r] = new Vector3(posX, posY, posZ);
            }

            //算出單邊的行數
            var colSingleSide = (col - col % 2) / 2;

            //往右延伸的起始索引
            var leftStartIndex = (col - 1) - colSingleSide;

            //在行數為奇數的情況下，陣列中間整行將不會被列入計算。
            //因此先進行賦值
            if(onOddColumns && col > 1)
            {
                for(int r = 0; r < row; r++)
                {
                    obj.positions[colSingleSide, r] = rowStartPos[r];
                }
            }

            //計算positions的相對座標
            float degree = 0;
            float ladderOffsetY = spaceY * ladder.floatValue;
            for(int c = 1; c <= colSingleSide; c++)
            {
                //在此判斷區塊控制通用參數
                if(c == 1)
                {
                    if(onOddColumns)
                    {
                        degree = bend.floatValue * 0.5f;
                    }
                }
                else
                {
                    degree += bend.floatValue;
                    //ladderOffsetY += ladderOffsetY;
                }

                for(int r = 0; r < row; r++)
                {
                    if(c == 1)
                    {
                        //區隔開行數在偶數與奇數情況下的狀況
                        if(onOddColumns)
                        {
                            //以物件座標為原始位置往右向外延伸的座標運算
                            obj.positions[colSingleSide - c, r] = rowStartPos[r] + GetPosition(degree, spaceX, true) + Vector3.down * ladderOffsetY;

                            //以物件座標為原始位置往左向外延伸的座標運算
                            obj.positions[leftStartIndex + c, r] = rowStartPos[r] + GetPosition(degree, spaceX, false) + Vector3.up * ladderOffsetY;
                        }
                        else
                        {
                            //以物件座標為原始位置往右向外延伸的座標運算
                            obj.positions[colSingleSide - c, r] = rowStartPos[r] + Vector3.right * (spaceX * 0.5f) + Vector3.down * (ladderOffsetY * 0.5f);

                            //以物件座標為原始位置往左向外延伸的座標運算
                            obj.positions[leftStartIndex + c, r] = rowStartPos[r] + Vector3.left * (spaceX * 0.5f) + Vector3.up * (ladderOffsetY * 0.5f);
                        }
                    }
                    else
                    {
                        //以物件座標為原始位置往右向外延伸的座標運算
                        obj.positions[colSingleSide - c, r] = obj.positions[colSingleSide - c + 1, r] + GetPosition(degree, spaceX, true) + Vector3.down * ladderOffsetY;

                        //以物件座標為原始位置往右向外延伸的座標運算
                        obj.positions[leftStartIndex + c, r] = obj.positions[leftStartIndex + c - 1, r] + GetPosition(degree, spaceX, false) + Vector3.up * ladderOffsetY;
                    }
                }
            }

            //計算positions的世界座標
            for(int c = 0; c < obj.positions.GetLength(0); c++)
            {
                for(int r = 0; r < obj.positions.GetLength(1); r++)
                {
                    obj.positions[c, r] = tran.TransformPoint(obj.positions[c, r]);
                }
            }
        }


        /// <summary>
        /// 依據代入的角度與長度取得新的座標點
        /// </summary>
        Vector3 GetPosition(float degree, float length, bool rightSide = true)
        {
            float radians = degree * Mathf.Deg2Rad;

            if(rightSide)
            {
                return new Vector3(length * Mathf.Cos(radians), 0, length * Mathf.Sin(radians));
            }
            else
            {
                return new Vector3(-length * Mathf.Cos(radians), 0, length * Mathf.Sin(radians));
            }
        }


        /// <summary>
        /// 繪製場景的預覽控制項
        /// </summary>
        void DrawPreviewHandlers()
        {
            var col = obj.positions.GetLength(0);
            var row = obj.positions.GetLength(1);
            var cubeSize = Vector3.one * Mathf.Max(width.floatValue, height.floatValue) * 0.01f;

            for(int c = 0; c < col; c++)
            {
                for(int r = 0; r < row; r++)
                {
                    Handles.color = colors[4];
                    if(c > 0)
                    {
                        Handles.DrawLine(obj.positions[c - 1, r], obj.positions[c, r]);
                    }

                    if(r > 0)
                    {
                        Handles.DrawLine(obj.positions[c, r - 1], obj.positions[c, r]);
                    }

                    Handles.color = colors[5];
                    Handles.DrawWireCube(obj.positions[c, r], cubeSize);
                }
            }
            Handles.color = Color.white;
        }

        #endregion


        #region >>  物件的銷毀與建立

        void Generate()
        {
            Clear();

            var col = obj.positions.GetLength(0);
            var row = obj.positions.GetLength(1);
            obj.clone = new GameObject[col * row];

            GameObject inst;
            var counter = 0;
            for(int c = 0; c < col; c++)
            {
                for(int r = 0; r < row; r++)
                {
                    string goName = string.Format("{0} - {1}", (c + 1).ToString().PadLeft(2, '0'), (r + 1).ToString().PadLeft(2, '0'));

                    inst = Instantiate(obj.go, obj.positions[c, r], obj.transform.rotation, obj.transform);
                    inst.name = goName;
                    obj.clone[counter] = inst;
                    counter++;
                }
            }
        }


        void Clear()
        {
            foreach(var go in obj.clone)
            {
                if(go != null)
                {
                    DestroyImmediate(go);
                }
            }
            obj.clone = new GameObject[0];
        }

        #endregion

    }
}