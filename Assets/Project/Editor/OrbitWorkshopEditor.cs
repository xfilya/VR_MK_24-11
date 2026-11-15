using System.Linq;
using OrbitLanes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OrbitLanes.Editor
{
    public class OrbitPalette : EditorWindow
    {
        const string Kit = "Assets/Project/Kit/";
        const string Scene = "Assets/Project/Scenes/VR_MK_1.unity";
        VisualElement selectionCard;
        UnityEditor.Editor detailEditor;
        GameObject displayedSelection;

        void Update()
        {
            if (selectionCard != null && displayedSelection != Selection.activeGameObject) ShowSelected();
        }
        void OnDisable()
        {
            if (detailEditor) DestroyImmediate(detailEditor);
        }

        void ShowSelected()
        {
            if (selectionCard == null) return;
            selectionCard.Unbind();
            selectionCard.Clear();
            if (detailEditor) DestroyImmediate(detailEditor);
            var obj = Selection.activeGameObject;
            displayedSelection = obj;
            if (!obj) return;
            Component part = obj.GetComponentInParent<OrbitBall>();
            if (!part) part = obj.GetComponentInParent<OrbitPin>();
            if (!part) part = obj.GetComponentInParent<OrbitSwitch>();
            if (!part) part = obj.GetComponentInParent<OrbitTheme>();
            if (!part) return;
            detailEditor = UnityEditor.Editor.CreateEditor(part);
            var panel = detailEditor.CreateInspectorGUI();
            panel.Bind(detailEditor.serializedObject);
            selectionCard.Add(panel);
        }
        [MenuItem("Мастерская/Орбита — набор деталей")]
        public static void Open()
        {
            var window = GetWindow<OrbitPalette>("Орбита · детали");
            window.minSize = new Vector2(300, 350);
        }
        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var scroll = new ScrollView();
            rootVisualElement.Add(scroll);
            OrbitInspector.Layout(scroll);
            selectionCard = new VisualElement();
            scroll.Add(selectionCard);
            ShowSelected();
            Fill(scroll);
        }
        public static void Fill(VisualElement root)
        {
            OrbitInspector.Title(root,"ОРБИТА","Конструктор дорожки");
            var copy = new Button(() =>
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Project/Scenes/Моя орбита.unity");
                if (AssetDatabase.CopyAsset(Scene, path)) EditorSceneManager.OpenScene(path);
            }) {text="Создать свою копию уровня"};
            root.Add(copy);
            root.Add(new HelpBox("Перетащи деталь за название на дорожку. Кнопка + ставит её в готовую позицию. Ctrl+Z отменяет действие.", HelpBoxMessageType.Info));
            Section(root,"Шары",new[]{"Comet","Solar","Nebula"},new[]{"Комета","Мандарин","Туманность"});
            Section(root,"Кегли",new[]{"Classic","Beacon","Rocket","Rack"},new[]{"Классика","Маяк","Ракета","Пирамида из 10"});
            Section(root,"Пульт и оформление",new[]{"Practice","Reset","Lights","Music","Signal","Decor","Lane"},new[]{"Пробный бросок","Новый матч","Смена темы","Музыка","Своя надпись","Панорама станции","Дорожка"});
        }
        static void Section(VisualElement parent,string title,string[] files,string[] names)
        {
            OrbitInspector.Heading(parent,title);
            for (int i=0;i<files.Length;i++)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+files[i]+".prefab");
                if (!prefab) continue;
                var row=new VisualElement(); row.style.flexDirection=FlexDirection.Row; row.style.marginBottom=3;
                var chip=new Label(names[i]); chip.style.flexGrow=1; chip.style.paddingLeft=9;
                chip.style.unityTextAlign=TextAnchor.MiddleLeft; chip.style.backgroundColor=new Color(.15f,.25f,.28f);
                chip.RegisterCallback<MouseDownEvent>(e =>
                {
                    if (e.button!=0) return;
                    DragAndDrop.PrepareStartDrag(); DragAndDrop.objectReferences=new Object[]{prefab};
                    DragAndDrop.StartDrag(prefab.name); e.StopPropagation();
                });
                row.Add(chip);
                row.Add(new Button(()=>Place(prefab)){text="+",tooltip="Добавить в сцену"});
                parent.Add(row);
            }
        }
        static void Place(GameObject prefab)
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Добавляй детали после выхода из Play."); return; }
            var club=Object.FindAnyObjectByType<OrbitClub>();
            if (prefab.GetComponent<OrbitTheme>() && club && club.decor)
            {
                Undo.RegisterFullObjectHierarchyUndo(club.decor.gameObject,"Сменить оформление");
                club.decor.Next();
                PrefabUtility.RecordPrefabInstancePropertyModifications(club.decor);
                EditorSceneManager.MarkSceneDirty(club.gameObject.scene);
                Selection.activeGameObject=club.decor.gameObject;
                return;
            }
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(go,"Добавить "+prefab.name);
            Vector3 slot=Vector3.zero;
            var ball = go.GetComponent<OrbitBall>();
            if (ball) slot=new Vector3(-.84f,1.01f,.37f);
            else if (prefab.name=="Lane") slot = Vector3.right * (3.4f * (Object.FindObjectsByType<Transform>().Count(t => t.name == "Lane") - 1));
            else if (go.GetComponent<OrbitPin>()) slot=new Vector3(.7f,.012f,5.9f);
            else if (go.name=="Rack") slot=new Vector3(0,.012f,3.8f);
            else if (go.GetComponent<OrbitSwitch>()) slot=new Vector3(-.75f,.86f,.42f);
            if (!go.GetComponent<OrbitTheme>())
            {
                var occupied=Object.FindObjectsByType<Transform>();
                for(int n=0;n<32;n++)
                {
                    Vector3 world=club?club.transform.TransformPoint(slot):slot;
                    bool overlaps=System.Array.Exists(occupied,t=>t!=go.transform && !t.IsChildOf(go.transform) &&
                        (t.GetComponent<OrbitBall>()||t.GetComponent<OrbitPin>()||t.GetComponent<OrbitSwitch>()) &&
                        (t.position-world).sqrMagnitude < .28f*.28f);
                    if (!overlaps) break;
                    slot.z+=ball ? -.49f : .34f;
                }
            }
            go.transform.position=club?club.transform.TransformPoint(slot):slot;
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
            if (ball && club)
            {
                SetPracticeBall(club,ball);
                if (club.ballStand)
                {
                    var stand = Object.Instantiate(club.ballStand,club.transform);
                    stand.name = "Подставка · " + prefab.name;
                    stand.position = go.transform.position - club.transform.up * 1.01f;
                    Undo.RegisterCreatedObjectUndo(stand.gameObject,"Добавить подставку");
                }
            }
            Selection.activeGameObject=go;
        }
        internal static void SetPracticeBall(OrbitClub club,OrbitBall ball)
        {
            Undo.RecordObject(club,"Выбрать шар для пробного броска");
            club.practiceBall=ball;
            PrefabUtility.RecordPrefabInstancePropertyModifications(club);
            EditorSceneManager.MarkSceneDirty(club.gameObject.scene);
        }
    }

    [CustomEditor(typeof(OrbitClub))]
    public class OrbitInspector : UnityEditor.Editor
    {
        public static void Layout(VisualElement root)
        {
            root.style.paddingLeft=8;root.style.paddingRight=8;root.style.paddingTop=8;root.style.paddingBottom=12;
        }
        public static void Title(VisualElement root,string name,string detail)
        {
            var band=new VisualElement(); band.style.backgroundColor=new Color(.066f,.16f,.21f);
            band.style.borderLeftWidth=4; band.style.borderLeftColor=new Color(.96f,.55f,.31f);
            band.style.paddingLeft=12; band.style.paddingTop=8; band.style.paddingBottom=9; band.style.marginBottom=8;
            var title=new Label(name);title.style.fontSize=20; title.style.unityFontStyleAndWeight=FontStyle.Bold;
            title.style.color=new Color(.98f,.83f,.59f);band.Add(title);
            var sub=new Label(detail);sub.style.color=new Color(.65f,.84f,.82f);band.Add(sub);root.Add(band);
        }
        public static void Heading(VisualElement root,string s)
        {
            var label=new Label(s);label.style.unityFontStyleAndWeight=FontStyle.Bold;label.style.marginTop=9;label.style.marginBottom=4;root.Add(label);
        }
        protected VisualElement Page(string title,string help)
        {
            var root=new VisualElement();Layout(root); Title(root,title,help);return root;
        }
        protected void Field(VisualElement root,string id,string label)
        {
            root.Add(new PropertyField(serializedObject.FindProperty(id),label));
        }
        protected void MessageField(VisualElement root,string id,string label,System.Func<int,bool> visible)
        {
            var mode = serializedObject.FindProperty(id);
            var message = new PropertyField(serializedObject.FindProperty("message"), label);
            root.Add(message);
            void Refresh(SerializedProperty property) =>
                message.style.display = visible(property.enumValueIndex) ? DisplayStyle.Flex : DisplayStyle.None;
            message.TrackPropertyValue(mode,Refresh);
            Refresh(mode);
        }
        public override VisualElement CreateInspectorGUI()
        {
            var root=Page("Матч","Боулинг на орбите");
            Field(root,"throwsPerRound","Бросков в раунде");Field(root,"settleSeconds","Пауза после удара, с");Field(root,"musicLevel","Громкость музыки");
            Heading(root,"Детали для занятия");OrbitPalette.Fill(root);return root;
        }
    }
    [CustomEditor(typeof(OrbitBall))]
    public class BallEditor : OrbitInspector
    {
        public override VisualElement CreateInspectorGUI()
        {
            var r=Page("Шар","Сожми боковую кнопку и отпусти на замахе");
            Field(r,"speed","Скорость броска ×"); Field(r,"punch","Толчок кегли"); Field(r,"mass","Масса шара, кг");Field(r,"trailTime","Длина следа, с");
            var ball=(OrbitBall)target;
            if (ball.gameObject.scene.IsValid() && !EditorApplication.isPlaying)
                r.Add(new Button(() => {var club=Object.FindAnyObjectByType<OrbitClub>();if(club) OrbitPalette.SetPracticeBall(club,ball);}){text="Выбрать для пробного броска"});
            return r;
        }
    }
    [CustomEditor(typeof(OrbitPin))]
    public class PinEditor : OrbitInspector
    {
        public override VisualElement CreateInspectorGUI()
        {
            var r=Page("Кегля","Очки начисляются один раз за падение");
            Field(r,"mass","Масса кегли, кг");Field(r,"points","Очки за попадание");
            Heading(r,"Когда упадёт");Field(r,"reaction","Что произойдёт");
            MessageField(r,"reaction","Надпись на табло",i => i==(int)HitReaction.Celebrate || i==(int)HitReaction.Message);
            var more=new Foldout{text="Дополнительное событие",value=false};Field(more,"onKnocked","При падении");r.Add(more);return r;
        }
    }
    [CustomEditor(typeof(OrbitTheme))]
    public class ThemeEditor : OrbitInspector
    {
        public override VisualElement CreateInspectorGUI()
        {
            var r=Page("Панорама","Цвета станции и пейзаж за дорожкой");
            Field(r,"theme","Оформление");
            r.Add(new Button(()=>{var t=(OrbitTheme)target;Undo.RegisterFullObjectHierarchyUndo(t.gameObject,"Оформление");t.Apply();foreach(var renderer in t.GetComponentsInChildren<Renderer>()) PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);EditorUtility.SetDirty(t);}){text="Показать в сцене"});return r;
        }
    }
    [CustomEditor(typeof(OrbitSwitch))]
    public class SwitchEditor : OrbitInspector
    {
        public override VisualElement CreateInspectorGUI()
        {
            var r=Page("Кнопка","Укажи контроллером и сожми рукоять");
            Field(r,"action","При нажатии");
            MessageField(r,"action","Текст для табло",i => i==(int)SwitchAction.Message);
            r.TrackPropertyValue(serializedObject.FindProperty("action"),p =>
            {
                var button=(OrbitSwitch)target;
                if (!button || !button.legend) return;
                Undo.RecordObject(button.legend,"Подпись кнопки");
                button.UpdateLegend();
                PrefabUtility.RecordPrefabInstancePropertyModifications(button.legend);
            });
            var more=new Foldout{text="Дополнительное событие",value=false};Field(more,"onPress","При нажатии");r.Add(more);return r;
        }
    }
}
