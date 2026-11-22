using NUnit.Framework;
using UnityEngine;

public class GameOverDialogueTests
{
    DialogueData bankrupt;
    DialogueData fallback;

    [SetUp]
    public void SetUp()
    {
        bankrupt = ScriptableObject.CreateInstance<DialogueData>();
        fallback = ScriptableObject.CreateInstance<DialogueData>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(bankrupt);
        Object.DestroyImmediate(fallback);
    }

    [Test]
    public void Pick_UsesTheEndingForThatScenario()
    {
        var endings = new DialogueData[10];
        endings[5] = bankrupt;

        Assert.AreSame(bankrupt, GameOverDialogue.Pick(endings, fallback, 5));
    }

    [Test]
    public void Pick_FallsBackWhenTheEndingIsEmptyOrOutOfRange()
    {
        var endings = new DialogueData[10];
        endings[5] = bankrupt;

        Assert.AreSame(fallback, GameOverDialogue.Pick(endings, fallback, 2), "empty slot");
        Assert.AreSame(fallback, GameOverDialogue.Pick(endings, fallback, -1), "no ending recorded");
        Assert.AreSame(fallback, GameOverDialogue.Pick(endings, fallback, 99), "past the array");
        Assert.AreSame(fallback, GameOverDialogue.Pick(null, fallback, 5), "no array at all");
    }
}
