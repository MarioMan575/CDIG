public struct RecipeState
{
    public BreadState BreadState;
    public bool IsSartenLista;
    public bool HasMezclaDulce;
    public bool HasTorrija;

    public RecipeState(BreadState breadState, bool isSartenLista, bool hasMezclaDulce)
    {
        BreadState = breadState;
        IsSartenLista = isSartenLista;
        HasMezclaDulce = hasMezclaDulce;
        HasTorrija = breadState == BreadState.Torrija;
    }
}
