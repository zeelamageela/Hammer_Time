using NUnit.Framework;

/// <summary>Each event's own ice.</summary>
public class VenueIceTests
{
    static void Sheet(VenueBackground bg, int seed, int venue, int visit, out float speed, out float curl)
    {
        VenueIce.Sheet((int)bg, seed, venue, visit, out speed, out curl);
    }

    [Test]
    public void EverySheetStaysNearItsKind()
    {
        float speedSpread = (1f + VenueIce.CharacterSpeed) * (1f + VenueIce.VisitSpeed) - 1f;
        float curlSpread = (1f + VenueIce.CharacterCurl) * (1f + VenueIce.VisitCurl) - 1f;
        foreach (VenueBackground bg in System.Enum.GetValues(typeof(VenueBackground)))
        {
            VenueIce.KindIce((int)bg, out float kindSpeed, out float kindCurl);
            for (int venue = 0; venue < 200; venue++)
            {
                Sheet(bg, 1234, venue, venue % 7, out float speed, out float curl);
                Assert.AreEqual(kindSpeed, speed, kindSpeed * speedSpread + 1e-4f);
                Assert.AreEqual(kindCurl, curl, kindCurl * curlSpread + 1e-4f);
            }
        }
    }

    [Test]
    public void OutdoorsIsSlowAndStraightTheStadiumFastAndCurly()
    {
        for (int venue = 0; venue < 50; venue++)
        {
            Sheet(VenueBackground.Outdoor, 99, venue, 1, out float outSpeed, out float outCurl);
            Sheet(VenueBackground.Club, 99, venue, 1, out float clubSpeed, out float clubCurl);
            Sheet(VenueBackground.Stadium, 99, venue, 1, out float bigSpeed, out float bigCurl);
            Assert.Less(outSpeed, clubSpeed);
            Assert.Less(clubSpeed, bigSpeed);
            Assert.Less(outCurl, clubCurl);
            Assert.Less(clubCurl, bigCurl);
        }
    }

    [Test]
    public void AnEventKeepsItsCharacterAndShiftsOnlyALittleBetweenVisits()
    {
        Sheet(VenueBackground.Club, 7, 3, 1, out float speed, out float curl);
        Sheet(VenueBackground.Club, 7, 3, 1, out float again, out float curlAgain);
        Assert.AreEqual(speed, again);
        Assert.AreEqual(curl, curlAgain);
        for (int visit = 2; visit < 30; visit++)
        {
            Sheet(VenueBackground.Club, 7, 3, visit, out float s, out float c);
            Assert.AreEqual(speed, s, 2.2f * VenueIce.VisitSpeed * 1.05f);
            Assert.AreEqual(curl, c, 2.2f * VenueIce.VisitCurl * 1.1f);
        }
    }

    [Test]
    public void EventsAndCareersDiffer()
    {
        Sheet(VenueBackground.Club, 7, 3, 1, out float speed, out _);
        Sheet(VenueBackground.Club, 7, 4, 1, out float otherEvent, out _);
        Sheet(VenueBackground.Club, 8, 3, 1, out float otherCareer, out _);
        Assert.AreNotEqual(speed, otherEvent);
        Assert.AreNotEqual(speed, otherCareer);
    }
}
