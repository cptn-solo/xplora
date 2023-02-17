using Google.Apis.Sheets.v4.Data;

namespace Assets.Scripts.Data
{

    /// <summary>
    /// Refer to Hero fields (mostly matches field names)
    /// Only damage min/max should be converted to Range sctructure one day
    /// </summary>
    public enum SpecOption
    {
        NA = 0,
        DamageRange = 100, //Min-Max // DamageMin+DamageMax in Hero
        DefenceRate = 200,
        AccuracyRate = 300,
        DodgeRate = 400,
        Health = 500,
        Speed = 600,
        UnlimitedStaminaTag = 700, // not present in Hero fields
    }
}