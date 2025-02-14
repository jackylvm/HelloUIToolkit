using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using SerializedObject = UnityEditor.SerializedObject;

public class SceneObjectWindow : EditorWindow
{
    [MenuItem("Tool/SceneObjectWindow")]
    public static void ShowExample()
    {
        var wnd = GetWindow<SceneObjectWindow>();
        wnd.titleContent = new GUIContent("SceneObjectWindow");
    }

    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = null;
    
    // 这些都是编辑器里面拖拽的UI对象,通过root.Q查找到它们
    private ObjectField topObjectField;
    private Button btnCreate;
    private Button btnRefresh;
    private ListView gameObjects;
    private TextField selectionObjectName;
    private Vector3Field selectionObjectPosition;
    private IntegerField selectionObjectProperty;
    private TextField selectionObjectSyncValue;

    // 列表保存的场景上的对象
    private GameObject[] objectsInScene;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // 代码添加的组件
        var helpBox01 = new HelpBox("这是一个Help Box,只是演示用!", HelpBoxMessageType.None);
        var helpBox02 = new HelpBox("这是一个Help Box,只是演示用!", HelpBoxMessageType.Info);
        var helpBox03 = new HelpBox("这是一个Help Box,只是演示用!", HelpBoxMessageType.Warning);
        var helpBox04 = new HelpBox("这是一个Help Box,只是演示用!", HelpBoxMessageType.Error);

        var objectInfo = root.Q<VisualElement>("ObjectInfo");
        objectInfo.Add(helpBox01);
        objectInfo.Add(helpBox02);
        objectInfo.Add(helpBox03);
        objectInfo.Add(helpBox04);

        // 查找对象
        topObjectField = root.Q<ObjectField>("TopObjectField");
        topObjectField.objectType = typeof(GameObject);
        topObjectField.allowSceneObjects = false;

        btnCreate = root.Q<Button>("BtnCreate");
        btnCreate.clicked += OnBtnCreateClicked;

        btnRefresh = root.Q<Button>("BtnRefresh");
        btnRefresh.clicked += OnBtnRefreshClicked;

        gameObjects = root.Q<ListView>("GameObjects");
        gameObjects.makeItem = OnGameObjectsListViewMakeItem;
        gameObjects.bindItem = OnGameObjectsListViewBindItem;
        gameObjects.selectionChanged += OnGameObjectsListViewSelectionChanged;

        selectionObjectName = root.Q<TextField>("FieldGameObjectName");
        selectionObjectName.bindingPath = "m_Name";

        selectionObjectPosition = root.Q<Vector3Field>("FieldGameObjectPosition");
        selectionObjectPosition.bindingPath = "m_LocalPosition";

        selectionObjectProperty = root.Q<IntegerField>("FieldGameObjectProperty");
        selectionObjectProperty.bindingPath = "theCount";

        selectionObjectSyncValue = root.Q<TextField>("FieldSyncValue");
        selectionObjectSyncValue.RegisterValueChangedCallback(OnSyncValueChanged);
    }

    private void OnSyncValueChanged(ChangeEvent<string> evt)
    {
        Debug.Log($"Old value: {evt.previousValue},New value: {evt.newValue}");
    }

    private void OnGameObjectsListViewSelectionChanged(IEnumerable<object> obj)
    {
        foreach (var selected in obj)
        {
            var go = selected as GameObject;
            Selection.activeObject = go;

            // 方法1,没有绑定,不能同步更改
            // selectionObjectName.value = go.name;
            // selectionObjectPosition.value = go.transform.localPosition;

            // 方法2,绑定对象,同步更新数据
            // var so1 = new SerializedObject(go);
            // selectionObjectName.Bind(so1);
            // var so2 = new SerializedObject(go.transform);
            // selectionObjectPosition.Bind(so2);

            // 处理特定类型的对象
            MyTriangle tmp;
            if (go.TryGetComponent<MyTriangle>(out tmp))
            {
                var so = new SerializedObject(tmp);
                selectionObjectProperty.Bind(so);
            }
            else
            {
                selectionObjectProperty.Unbind();
            }
        }
    }

    private void OnGameObjectsListViewBindItem(VisualElement ve, int idx)
    {
        var label = ve as Label;
        var go = objectsInScene[idx];
        label.text = go.name;
    }

    private VisualElement OnGameObjectsListViewMakeItem()
    {
        var label = new Label
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleLeft,
                marginLeft = 5
            }
        };
        return label;
    }

    private void OnBtnRefreshClicked()
    {
        var activeScene = SceneManager.GetActiveScene();
        objectsInScene = activeScene.GetRootGameObjects();
        gameObjects.itemsSource = objectsInScene;
    }

    private void OnBtnCreateClicked()
    {
        if (topObjectField.value == null)
        {
            Debug.LogError("没有指定对象!");
            return;
        }

        var prefab = topObjectField.value as GameObject;
        var go = Instantiate<GameObject>(prefab);

        go.transform.position = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
    }
}