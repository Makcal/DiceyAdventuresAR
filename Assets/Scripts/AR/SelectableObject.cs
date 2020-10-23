namespace DiceyDungeonsAR.AR
{
    public interface ISelectableObject
    {
        bool IsSelected { get; set; }

        void OnSelectEnter();

        void OnSelectExit();
    }
}
