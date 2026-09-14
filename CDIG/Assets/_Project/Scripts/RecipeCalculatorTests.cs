using System;
using System.Collections.Generic;

public static class RecipeCalculatorTests
{
    public static void Main()
    {
        ProducesTorrijaOnlyWhenAllPreviousCombinationsArePossible();
        RecalculatesBackToEarlierStateWhenATargetIsLost();
        Console.WriteLine("RecipeCalculatorTests passed.");
    }

    private static void ProducesTorrijaOnlyWhenAllPreviousCombinationsArePossible()
    {
        var detected = new HashSet<RecipeElement>
        {
            RecipeElement.Plato,
            RecipeElement.Pan,
            RecipeElement.Leche,
            RecipeElement.Bandeja,
            RecipeElement.Huevo,
            RecipeElement.Aceite,
            RecipeElement.Sarten,
            RecipeElement.Azucar,
            RecipeElement.Canela
        };

        RecipeState state = RecipeCalculator.Calculate(detected);

        AssertEqual(BreadState.Torrija, state.BreadState, "all valid dependencies should produce Torrija");
        AssertTrue(state.IsSartenLista, "aceite + sarten should produce sarten lista");
        AssertTrue(state.HasMezclaDulce, "azucar + canela should produce mezcla dulce");
        AssertTrue(state.HasTorrija, "pan dulce + plato should produce final torrija");
    }

    private static void RecalculatesBackToEarlierStateWhenATargetIsLost()
    {
        var detected = new HashSet<RecipeElement>
        {
            RecipeElement.Plato,
            RecipeElement.Pan,
            RecipeElement.Leche,
            RecipeElement.Bandeja,
            RecipeElement.Huevo,
            RecipeElement.Aceite,
            RecipeElement.Sarten,
            RecipeElement.Azucar
        };

        RecipeState state = RecipeCalculator.Calculate(detected);

        AssertEqual(BreadState.Frito, state.BreadState, "without canela there is no mezcla dulce, so bread remains frito");
        AssertFalse(state.HasMezclaDulce, "missing canela should remove mezcla dulce");
        AssertFalse(state.HasTorrija, "missing mezcla dulce prevents torrija even if plato is visible");
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception(message + ". Expected: " + expected + ". Actual: " + actual);
        }
    }

    private static void AssertTrue(bool value, string message)
    {
        if (!value)
        {
            throw new Exception(message);
        }
    }

    private static void AssertFalse(bool value, string message)
    {
        if (value)
        {
            throw new Exception(message);
        }
    }
}
