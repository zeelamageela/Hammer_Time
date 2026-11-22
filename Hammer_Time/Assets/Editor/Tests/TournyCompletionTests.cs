using NUnit.Framework;

public class TournyCompletionTests
{
    static Tourny T(string name, int id, bool complete = false) =>
        new Tourny { name = name, id = id, complete = complete };

    // Mirrors the real scene data: empty shares id 0 with tournies[0]; locals share 50-52 with provQual.
    Tourny empty, fall, nextEvent, local52, qual50, qual51, qual52;
    Tourny[] locals;

    [SetUp]
    public void SetUp()
    {
        empty = T("Nothing Scheduled", 0);
        fall = T("Fall Rookie Invitational", 0, complete: true);   // played in week 1
        nextEvent = T("Jim Jumbo", 1);
        local52 = T("Underground Gambling Den", 52);
        qual50 = T("Tour Qualifier", 50);
        qual51 = T("Tour Qualifier", 51);
        qual52 = T("Tour Qualifier", 52);
        locals = new[] { T("Out on the Pond", 50), T("Club Night", 51), local52 };
    }

    [Test]
    public void ConsumesTheOtherRealEventsOffered()
    {
        var active = new[] { local52, qual50, nextEvent };

        var marked = TournyCompletion.MarkOffered(active, empty, locals);

        Assert.IsTrue(qual50.complete);
        Assert.IsTrue(nextEvent.complete);
        Assert.AreEqual(2, marked.Count);
    }

    [Test]
    public void LocalEvent_DoesNotConsumeQualifierWithSameId()
    {
        var active = new[] { local52, qual50, nextEvent };

        TournyCompletion.MarkOffered(active, empty, locals);

        Assert.IsFalse(qual51.complete, "qualifier 51 was never offered");
        Assert.IsFalse(qual52.complete, "qualifier 52 shares an id with the local on screen but was never offered");
        Assert.IsFalse(local52.complete);
    }

    [Test]
    public void EmptySlot_DoesNotTouchEventWithSameId()
    {
        fall.complete = false;      // as if it had not been played
        var active = new[] { empty, qual50, nextEvent };

        TournyCompletion.MarkOffered(active, empty, locals);

        Assert.IsFalse(empty.complete);
        Assert.IsFalse(fall.complete, "id 0 on the empty placeholder must not complete tournies[0]");
    }

    [Test]
    public void Undo_RestoresExactlyWhatWasMarked()
    {
        var active = new[] { empty, qual50, nextEvent };
        var marked = TournyCompletion.MarkOffered(active, empty, locals);

        TournyCompletion.Undo(marked);

        Assert.IsFalse(qual50.complete);
        Assert.IsFalse(nextEvent.complete);
        Assert.IsTrue(fall.complete, "a previously played event must stay complete after Back");
        Assert.AreEqual(0, marked.Count);
    }

    [Test]
    public void AlreadyCompleteOfferedEvent_IsNotMarkedAndSurvivesUndo()
    {
        // The selector can fall back to an already-finished event when nothing is left.
        var active = new[] { empty, fall, nextEvent };
        var marked = TournyCompletion.MarkOffered(active, empty, locals);

        TournyCompletion.Undo(marked);

        Assert.IsTrue(fall.complete);
    }

    [Test]
    public void NullsAndNullArrays_AreTolerated()
    {
        Assert.AreEqual(0, TournyCompletion.MarkOffered(null, empty, locals).Count);
        Assert.AreEqual(1, TournyCompletion.MarkOffered(new Tourny[] { null, nextEvent }, empty, null).Count);
        TournyCompletion.Undo(null);
    }
}
