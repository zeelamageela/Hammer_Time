using NUnit.Framework;
using UnityEngine;

/// <summary>The player's read of the ice.</summary>
public class PlayerReadTests
{
    [Test]
    public void NotesSurviveBeingStored()
    {
        var note = new PlayerRead.Note { speed = 1.16f, curl = 0.62f, visit = 4 };
        Assert.IsTrue(PlayerRead.TryUnpack(PlayerRead.Pack(note), out var back));
        Assert.AreEqual(note.speed, back.speed, 1e-4f);
        Assert.AreEqual(note.curl, back.curl, 1e-4f);
        Assert.AreEqual(note.visit, back.visit);
    }

    [Test]
    public void NothingStoredIsNoNotes()
    {
        Assert.IsFalse(PlayerRead.TryUnpack("", out _));
        Assert.IsFalse(PlayerRead.TryUnpack("fast;curly", out _));
    }

    [Test]
    public void ALineOnAnotherReadEndsSomewhereElse()
    {
        var real = RockPhysicsProfile.Arcade().WithIce(1.15f, 1.35f);
        var read = RockPhysicsProfile.Arcade().WithIce(1f, 1f);
        Vector2 throwIt = new Vector2(0f, 8.5f);
        var simulator = new TrajectorySimulator(0f, 0f) { Ice = read };
        var guessed = simulator.SimulateTrajectory(new Vector2(0f, -25f), throwIt, true);
        simulator.Ice = real;
        var actual = simulator.SimulateTrajectory(new Vector2(0f, -25f), throwIt, true);
        Assert.Greater(actual[actual.Count - 1].y - guessed[guessed.Count - 1].y, 2f);   // the faster sheet carries it further
    }

    [Test]
    public void TheIceButtonIsOfferedWheneverTheSheetIsQuiet()
    {
        Assert.IsTrue(IceReadPanel.Offered(GameState.REDTURN, false, false));
        Assert.IsTrue(IceReadPanel.Offered(GameState.YELLOWTURN, false, false));
        Assert.IsTrue(IceReadPanel.Offered(GameState.CHECKSCORE, false, false), "after a shot");
        Assert.IsFalse(IceReadPanel.Offered(GameState.CHECKSCORE, false, true), "rocks still moving");
        Assert.IsFalse(IceReadPanel.Offered(GameState.REDTURN, true, false), "rock in hand");
        Assert.IsFalse(IceReadPanel.Offered(GameState.SCORE, false, false));
        Assert.IsFalse(IceReadPanel.Offered(GameState.END, false, false));
    }
}
