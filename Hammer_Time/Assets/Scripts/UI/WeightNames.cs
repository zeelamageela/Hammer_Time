/// <summary>
/// The curler's name for a shot's weight, by where the stone would stop if it hit nothing (y on the sheet: far hog 0, tee 6.5, the
/// 12-foot ring 1.5 each side of it). Draw weights from guards to the back of the house also carry the 1-10 number.
/// Rename a weight or move a cut-off in the table; each entry covers the stones that stop short of its "to" value.
/// </summary>
public static class WeightNames
{
    const float Tee = 6.5f;
    const float House = 1.5f, EightFoot = 1.0f, FourFoot = 0.5f;   // ring radii
    const float Button = 0.2f;                                     // half the length of "button weight"
    const float Rock = 0.14f;                                      // a stone this far outside a ring still touches it

    struct Weight
    {
        public float to;
        public int number;   // 0 = not numbered
        public string name;
        public Weight(float to, int number, string name) { this.to = to; this.number = number; this.name = name; }
    }

    static readonly Weight[] Ladder =
    {
        new Weight(0f,                            0,  "Short"),
        new Weight(2f,                            1,  "Long Guard"),
        new Weight(3.5f,                          2,  "Guard"),
        new Weight(Tee - House - Rock,            3,  "Tight Guard"),
        new Weight(Tee - (House + EightFoot) / 2f,    4,  "Top 12"),
        new Weight(Tee - (EightFoot + FourFoot) / 2f, 5,  "Top 8"),
        new Weight(Tee - Button,                  6,  "Top 4"),
        new Weight(Tee + Button,                  7,  "Button"),
        new Weight(Tee + (EightFoot + FourFoot) / 2f, 8,  "Back 4"),
        new Weight(Tee + (House + EightFoot) / 2f,    9,  "Back 8"),
        new Weight(Tee + House + Rock,            10, "Back 12"),
        new Weight(10f,                           0,  "Hack"),
        new Weight(13f,                           0,  "Board"),
        new Weight(20f,                           0,  "Takeout"),
        new Weight(float.PositiveInfinity,        0,  "Peel"),
    };

    /// <summary>"7 - Button", "Peel": the call for a stone that would stop at this y.</summary>
    public static string For(float stopY)
    {
        int i = 0;
        while (i < Ladder.Length - 1 && stopY >= Ladder[i].to) i++;
        return Ladder[i].number > 0 ? Ladder[i].number + " - " + Ladder[i].name : Ladder[i].name;
    }
}
