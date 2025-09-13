using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public enum Gender
    {
        Unspecified = 0,
        Men = 1,
        Women = 2,
        Kids = 3
    }

    public enum ProductType
    {
        Unknown = 0,
        Tops = 1,
        Bottoms = 2
    }

    public enum ClothingSize
    {
        Unknown = 0,
        XS = 1,
        S = 2,
        M = 3,
        L = 4,
        XL = 5,
        XXL = 6
    }

    public enum CustomerGender
    {
        Unspecified = 0,
        Male = 1,
        Female = 2,
        Other = 3
    }
}


