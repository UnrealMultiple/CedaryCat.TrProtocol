using TrProtocol.Attributes;

namespace Terraria
{
    public class Recipe
    {
        public struct RequiredItemEntry
        {
            public int itemIdOrRecipeGroup;
            [Int7BitEncoded]
            public int stack;
            
            public override string ToString()
            {
                return $"{{ItemIdOrRecipeGroup: {itemIdOrRecipeGroup}, Stack: {stack}}}";
            }
        }
    }
}
