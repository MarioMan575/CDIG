using System.Collections.Generic;

public static class RecipeCalculator
{
    public static RecipeState Calculate(ISet<RecipeElement> detectedElements)
    {
        bool hasPan = detectedElements.Contains(RecipeElement.Pan);
        bool hasLeche = detectedElements.Contains(RecipeElement.Leche);
        bool hasHuevo = detectedElements.Contains(RecipeElement.Huevo);
        bool hasAceite = detectedElements.Contains(RecipeElement.Aceite);
        bool hasAzucar = detectedElements.Contains(RecipeElement.Azucar);
        bool hasCanela = detectedElements.Contains(RecipeElement.Canela);
        bool hasBandeja = detectedElements.Contains(RecipeElement.Bandeja);
        bool hasSarten = detectedElements.Contains(RecipeElement.Sarten);
        bool hasPlato = detectedElements.Contains(RecipeElement.Plato);

        bool isSartenLista = hasAceite && hasSarten;
        bool hasMezclaDulce = hasAzucar && hasCanela;

        BreadState breadState = BreadState.None;

        if (hasPan)
        {
            breadState = BreadState.Seco;

            if (hasLeche && hasBandeja)
            {
                breadState = BreadState.Mojado;
            }

            if (breadState == BreadState.Mojado && hasHuevo)
            {
                breadState = BreadState.Rebozado;
            }

            if (breadState == BreadState.Rebozado && isSartenLista)
            {
                breadState = BreadState.Frito;
            }

            if (breadState == BreadState.Frito && hasMezclaDulce)
            {
                breadState = BreadState.Dulce;
            }

            if (breadState == BreadState.Dulce && hasPlato)
            {
                breadState = BreadState.Torrija;
            }
        }

        return new RecipeState(breadState, isSartenLista, hasMezclaDulce);
    }
}
