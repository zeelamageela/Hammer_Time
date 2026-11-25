/// <summary>Adding up bonus sources (gear, training) into the single set of numbers a match adds to the player team's skills.</summary>
public static class SkillBonuses
{
    public static CareerStats Combine(CareerStats a, CareerStats b)
    {
        var sum = new CareerStats();
        if (a != null)
        {
            sum.weightAccuracy += a.weightAccuracy; sum.finesseAccuracy += a.finesseAccuracy; sum.aimAccuracy += a.aimAccuracy;
            sum.sweepStrength += a.sweepStrength; sum.sweepEndurance += a.sweepEndurance; sum.sweepCohesion += a.sweepCohesion;
        }
        if (b != null)
        {
            sum.weightAccuracy += b.weightAccuracy; sum.finesseAccuracy += b.finesseAccuracy; sum.aimAccuracy += b.aimAccuracy;
            sum.sweepStrength += b.sweepStrength; sum.sweepEndurance += b.sweepEndurance; sum.sweepCohesion += b.sweepCohesion;
        }
        return sum;
    }
}
