using Cysharp.Threading.Tasks;
using OcentraAI.LLMGames.Events;
using OcentraAI.LLMGames.Extensions;
using OcentraAI.LLMGames.GameModes;
using OcentraAI.LLMGames.Manager;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OcentraAI.LLMGames.UI
{
    [ExecuteAlways]
    public class ElementGameHolder<T,TData> : MonoBehaviourBase<ElementGameHolder<T,TData>> 
    {


        [OnValueChanged(nameof(OnValueChanged)), SerializeField] protected float PanelWidth = 10.0f;
        [OnValueChanged(nameof(OnValueChanged)), SerializeField] protected float PanelHeight = 5.0f;
        [OnValueChanged(nameof(OnValueChanged)), SerializeField] protected float ItemSpacing = 2.0f;
        [OnValueChanged(nameof(OnValueChanged)), SerializeField] protected Padding Padding = new Padding(0f, 0f, 0f, 0f);
        [OnValueChanged(nameof(OnValueChanged)), SerializeField] protected int ViewCapacity = 5;
        [SerializeField] protected Color GizmoColor = new Color(0, 1, 0, 0.2f);



        [SerializeField, ReadOnly] protected float MaxHeight;
        [SerializeField, ReadOnly] protected float TotalWidth;


        [SerializeField] protected ChildElementHolder<ChildElement<TData>, TData> Elements = new ChildElementHolder<ChildElement<TData>, TData>();

        [SerializeField] protected Button3DSimple LeftButton;
        [SerializeField] protected Button3DSimple RightButton;

        [SerializeField, ShowInInspector, ValueDropdown(nameof(GetAvailableValue)), LabelText(nameof(FilterContext)), OnValueChanged(nameof(OnValueChanged)), PropertyOrder(-1)]
        private int id = 0;


        public IEnumerable<ValueDropdownItem<int>> GetAvailableValue()
        {
            List<ValueDropdownItem<int>> dropdownItems = new List<ValueDropdownItem<int>>();
            if (FilterContext is GameGenre )
            {
                foreach (GameGenre genre in GameGenre.GetAll())
                {
                    dropdownItems.Add(new ValueDropdownItem<int>(genre.Name, genre.Id));
                }
            }


            return dropdownItems;
        }

        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public TData FilterContext
        {
            get
            {
                if (typeof(TData) == typeof(GameGenre))
                {
                    return (TData)(object)GameGenre.FromId(id);
                }

                return default;
            }

            set
            {
                if (value is ILabeledItem labeledItem)
                {
                    id = labeledItem.Id;
                }

            }
        }

        protected override void OnValidate()
        {
            Init();
            base.OnValidate();
        }

        protected override void Start()
        {
            if (typeof(TData) == typeof(GameGenre))
            {
                FilterContext = (TData)(object)GameGenre.None;
            }

            Init();
            base.Start();
        }

        public override void SubscribeToEvents()
        {

            EventRegistrar.Subscribe<Button3DSimpleClickEvent>(OnButton3DSimpleClick);
            base.SubscribeToEvents();
        }

        private async UniTask OnButton3DSimpleClick(Button3DSimpleClickEvent e)
        {
            IButton3DSimple button3DSimple = e.Button3DSimple;
            if (LeftButton != null && ReferenceEquals(button3DSimple, LeftButton))
            {
                Elements.ScrollLeft(FilterContext);

                OnValueChanged();
            }

            if (RightButton != null && ReferenceEquals(button3DSimple, RightButton))
            {
                Elements.ScrollRight(FilterContext);

                OnValueChanged();
            }
            ArcadeGameGenre arcadeSelectedGame = button3DSimple as ArcadeGameGenre;
            if (arcadeSelectedGame != null)
            {
                GameGenre filterContext = arcadeSelectedGame.GameGenre;
                FilterContext = (TData)(object)filterContext;
                Elements.FilterView(FilterContext);

                OnValueChanged();
            }

            await UniTask.Yield();
        }

        private void EnsureElementsInitialized()
        {
            if (Elements == null)
            {
                Elements = new ChildElementHolder<ChildElement<TData>, TData>(ViewCapacity);
            }
            else if (Elements.ViewCapacity != ViewCapacity)
            {

                Elements.ViewCapacity = ViewCapacity;
                Elements.Clear();
            }
        }

        public void Init()
        {
            EnsureElementsInitialized();

            if (transform.childCount == 0) return;
            LeftButton = transform.FindChildRecursively<Button3DSimple>(nameof(LeftButton));
            RightButton = transform.FindChildRecursively<Button3DSimple>(nameof(RightButton));


            MaxHeight = PanelHeight - Padding.Top - Padding.Bottom;


            if (Elements != null)
            {

                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    int instanceID = child.gameObject.GetInstanceID();
                    BoxCollider boxCollider = child.GetComponent<BoxCollider>();
                    T modeUI = child.GetComponent<T>();


                    if (modeUI != null)
                    {
                        TData data = default;
                        if (modeUI is GameModeUI gameModeUI)
                        {
                             data = (TData)(object)gameModeUI.GameModeType.GameGenre;
                        }

                        if (modeUI is LobbyHolderUI lobbyHolderUI)
                        {
                            data = (TData)(object)lobbyHolderUI.GameModeType.GameGenre;
                        }


                        TData filterContextData = data;

                        if (filterContextData != null)
                        {
                            if (boxCollider != null)
                            {
                                if (!Elements.Contains(instanceID))
                                {
                                    ChildElement<TData> childElement = new ChildElement<TData>(child, i, boxCollider, filterContextData);
                                    childElement.Resize(MaxHeight);
                                    Elements.Add(childElement);
                                }
                                else
                                {
                                    ChildElement<TData> childElement = Elements.GetItem(instanceID);
                                    childElement.Update(child, boxCollider, filterContextData);
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"GameModeType {data.GetType()} is not assignable to {typeof(TData)} for child: {child.name}");
                        }
                    }

                }

                if (Elements.ViewCapacity > Elements.Count)
                {
                    Elements.ViewCapacity = Elements.Count;
                }

                Elements.FilterView(FilterContext);

            }

            OnValueChanged();
        }




        private void OnValueChanged()
        {
            EnsureElementsInitialized();

            if (transform.childCount == 0) return;

            MaxHeight = PanelHeight - Padding.Top - Padding.Bottom;

            TotalWidth = Padding.Left + Padding.Right + (ItemSpacing * (Elements.ViewItems.Count - 1));

            foreach (ChildElement<TData> childElement in Elements.GetAll())
            {
                childElement.Resize(MaxHeight);
                childElement.SetActive(false);
            }

            foreach (ChildElement<TData> holderItem in Elements.ViewItems)
            {
                TotalWidth += holderItem.ElementSizeX;
            }

            if (TotalWidth > PanelWidth)
            {
                Debug.LogWarning("TotalWidth exceeds PanelWidth. Adjust ItemSpacing or element sizes.");
            }

            float startX = -PanelWidth * 0.5f + Padding.Left;

            foreach (ChildElement<TData> holderItem in Elements.ViewItems)
            {
                holderItem.Child.localPosition = new Vector3(startX + holderItem.ElementSizeX / 2f, 0f, 0f);
                startX += holderItem.ElementSizeX + ItemSpacing;
                holderItem.SetActive(true);
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            ResetElements();
            Init();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Elements?.Clear();
        }

        private void ResetElements()
        {
            if (Elements == null)
            {
                Elements = new ChildElementHolder<ChildElement<TData>, TData>(ViewCapacity);
            }
            else
            {
                Elements.Clear();
                Elements.ViewCapacity = ViewCapacity;
            }
        }



        private void OnDrawGizmosSelected()
        {
            Gizmos.color = GizmoColor;

            Vector3 containerCenter = transform.position;
            Vector3 containerSize3D = new Vector3(PanelWidth, PanelHeight, 0.1f);

            Matrix4x4 oldMatrix = Gizmos.matrix;

            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawCube(Vector3.zero, containerSize3D);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, containerSize3D);

            Gizmos.matrix = oldMatrix;
        }
    }
}
