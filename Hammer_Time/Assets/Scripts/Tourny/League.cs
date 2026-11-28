using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Who is in a new career: 32 teams, 16 from the player's country (nine of them at the player's club, seven the
/// champions of other clubs) and 16 from abroad (at least one per country). 15 of them are on tour; the sixteenth
/// place is the player's until earning it is built.
/// </summary>
public static partial class League
{
    public const int Teams = 32;
    public const int Clubmates = 8;
    public const int OtherClubs = 7;
    public const int TourTeams = 16;
    public const int ClubmatesOnTour = 2;
    public const int ChampionsOnTour = 4;
    public const int ForeignOnTour = 9;
    public const string DefaultCountry = "CAN";
    public const string DefaultClub = "Home Club";

    public static readonly string[] Countries =
        { "CAN", "USA", "SCO", "ENG", "SWE", "NOR", "DEN", "FIN", "GER", "SUI", "ITA", "JPN", "KOR", "CHN" };

    static readonly string[] CountryNames =
        { "Canada", "United States", "Scotland", "England", "Sweden", "Norway", "Denmark", "Finland", "Germany",
          "Switzerland", "Italy", "Japan", "South Korea", "China" };

    public static string CountryName(string code)
    {
        int i = Array.IndexOf(Countries, code);
        return i < 0 ? "" : CountryNames[i];
    }

    public static string ValidCountry(string code) => Array.IndexOf(Countries, code) < 0 ? DefaultCountry : code;

    /// <summary>A name and the countries it can come from; the first is the main one.</summary>
    public struct Name
    {
        public string name;
        public string[] countries;
        public bool From(string code) => Array.IndexOf(countries, code) >= 0;
    }

    static Name[] Parse(string[] list) => list.Select(line =>
    {
        int split = line.LastIndexOf(':');
        return new Name { name = line.Substring(0, split), countries = line.Substring(split + 1).Split(' ') };
    }).ToArray();

    public static Name[] Characters => Parse(CharacterList);
    public static Name[] Surnames => Parse(SurnameList);

    /// <summary>The home towns (clubs) of a country.</summary>
    public static string[] Towns(string country) => TownList
        .Where(line => line.StartsWith(ValidCountry(country) + ":", StringComparison.Ordinal))
        .Select(line => line.Substring(line.IndexOf(':') + 1)).ToArray();

    public static string RandomTown(string country, Func<int, int> next)
    {
        var towns = Towns(country);
        return towns.Length == 0 ? DefaultClub : towns[next(towns.Length)];
    }

    public enum Kind { Player, Clubmate, ClubChampion, Foreign }

    public class Entry
    {
        public string name;
        public string country;
        public string club;        // home country teams only; teams from abroad have none
        public Kind kind;
        public bool onTour;
        public bool character;     // one of the game's own characters, not a surname from the list
        public int strength;       // 0-100 starting rating, spread over the season's stat range
    }

    /// <summary>
    /// Builds the league. Entry 0 is the player. <paramref name="next"/> returns a number from 0 up to, not including,
    /// its argument.
    /// </summary>
    public static List<Entry> Build(string playerTeam, string homeCountry, string homeClub, Func<int, int> next)
    {
        homeCountry = ValidCountry(homeCountry);
        homeClub = string.IsNullOrWhiteSpace(homeClub) ? RandomTown(homeCountry, next) : homeClub.Trim();

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { (playerTeam ?? "").Trim() };
        var characters = Shuffled(Characters, next);
        var surnames = Shuffled(Surnames, next);
        var league = new List<Entry>
        {
            new Entry { name = playerTeam, country = homeCountry, club = homeClub, kind = Kind.Player }
        };

        Entry Add(string country, Kind kind, string club)
        {
            // characters whose main country this is, then other characters who fit, then surnames in the same order
            bool character = true;
            string name = Take(characters, country, used);
            if (name == null)
            {
                character = false;
                name = Take(surnames, country, used);
            }
            if (name == null) name = CountryName(country) + " " + (league.Count + 1);

            var entry = new Entry { name = name, country = country, club = club, kind = kind, character = character };
            league.Add(entry);
            return entry;
        }

        for (int i = 0; i < Clubmates; i++) Add(homeCountry, Kind.Clubmate, homeClub);

        // abroad: every other country once, thinnest first so it gets its own characters, then the spare places
        var foreign = Countries.Where(c => c != homeCountry)
                               .OrderBy(c => characters.Count(n => n.From(c))).ToList();
        var spare = Shuffled(foreign.ToArray(), next);
        int foreignPlaces = Teams - 1 - Clubmates - OtherClubs;
        for (int i = 0; foreign.Count < foreignPlaces; i++) foreign.Add(spare[i % spare.Length]);
        foreach (var country in foreign) Add(country, Kind.Foreign, "");

        // the other clubs: different towns, never the player's own
        var otherTowns = Shuffled(Towns(homeCountry), next)
                         .Where(t => !t.Equals(homeClub, StringComparison.OrdinalIgnoreCase)).ToList();
        for (int i = 0; i < OtherClubs; i++)
            Add(homeCountry, Kind.ClubChampion, i < otherTowns.Count ? otherTowns[i] : "");

        PutOnTour(league, Kind.Clubmate, ClubmatesOnTour, next);
        PutOnTour(league, Kind.ClubChampion, ChampionsOnTour, next);
        PutOnTour(league, Kind.Foreign, ForeignOnTour, next);

        foreach (var e in league)
        {
            if (e.kind == Kind.Player) continue;
            int floor = e.onTour ? 60 : e.kind == Kind.Clubmate ? 10 : 35;
            e.strength = floor + next(36);
        }
        return league;
    }

    static void PutOnTour(List<Entry> league, Kind kind, int places, Func<int, int> next)
    {
        // characters get the tour places first: they are the faces of the tour
        var order = Shuffled(league.Where(e => e.kind == kind).ToArray(), next)
                    .OrderByDescending(e => e.character);
        foreach (var e in order.Take(places)) e.onTour = true;
    }

    static string Take(Name[] names, string country, HashSet<string> used)
    {
        for (int pass = 0; pass < 2; pass++)
            foreach (var n in names)
            {
                bool fits = pass == 0 ? n.countries[0] == country : n.From(country);
                if (fits && used.Add(n.name)) return n.name;
            }
        return null;
    }

    static T[] Shuffled<T>(T[] source, Func<int, int> next)
    {
        var a = (T[])source.Clone();
        for (int i = a.Length - 1; i > 0; i--)
        {
            int j = next(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }
}
