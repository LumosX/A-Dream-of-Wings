using System.Collections.Generic;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

public static class DateStuff {
    // I'll just shove these here because who cares
    public const int STARTING_DAY = 14;
    public const int STARTING_MONTH = 2;
    public const int STARTING_YEAR = 3279;

    private static int absoluteStartingDay => (STARTING_YEAR - 1) * MONTHS_PER_YEAR * DAYS_PER_MONTH +
                                              (STARTING_MONTH - 1) * DAYS_PER_MONTH +
                                              (STARTING_DAY - 1);

    public const int DAYS_PER_MONTH = 22;
    public const int MONTHS_PER_YEAR = 10;

    private static int daysPerYear = DAYS_PER_MONTH * MONTHS_PER_YEAR;

    // tuples are pretty nifty, aren't they
    public static (int, int, int) GetDateFormat(int numDaysPassed) {
        var absoluteDay = absoluteStartingDay + numDaysPassed;
    
        var curYear = absoluteDay / daysPerYear;
        var curMonth = (absoluteDay - daysPerYear * curYear) / DAYS_PER_MONTH;
        var curDay = (absoluteDay % daysPerYear) % DAYS_PER_MONTH;
        
        return (curDay + 1, curMonth + 1, curYear + 1);
    }
    
    
    public static Dictionary<int, string> DailyNews = new Dictionary<int, string> {
        // All right, I'll write unique ones for most of 'em.
        {0, "----- ????"},
        
        {1, "Bad news again. Scriptorium forces have lost Talikka, a small frontier world. The Circle's fleets " +
            "advance, although they are still far."},
        {2, "There's some news this morning: The Circle continues to advance. Grand Master Chenot Chen has mustered every world of the Scriptorium."},
        {3, "This time it's good news for a change: Circle forces around the Raddik Belt -- still far -- have been driven back."},
        {4, "No news this morning. Perhaps we've still got a chance."},
        {5, "Bad news again. The newly-reformed First Fleet has taken heavy damage in a battle around the Catharsis Nebula."},
        {6, "A Circle battlefleet has been destroyed. Apparently another one has been heavily damaged after a voidjump gone wrong."},
        {7, "No news this morning."},
        {8, "Still no news this morning, but something should be happening soon."},
        {9, "The Eighth Fleet has won a pyrrhic victory against the Circle in the skies above Feleheim. The Circle's advance is delayed."},
        {10, "Several additional Circle battlefleets have been sighted in Scriptorium space."},
        {11, "No news this morning."},
        // WAR GOES BAD HERE
        {12, "This morning bears terrible news. The entirety of the Fourth Fleet has been destroyed. They were caught in an " +
             "ambush by a Circle battlefleet. The war has once again taken a turn for the worse. Grand Master Chenot Chen advises that " +
             "we remain uplifted and certain in our eventual victory." +
             "\n\n" +
             "<b>Things are going to get noticeably worse now.</b>"},
        {13, "The majority of the Scriptorium's fleets have retreated to the Great Blockade, which shall stop the enemy's advance in " +
             "its tracks. The Circle is to break upon the Blockade like a wave on the shore."},
        {14, "No news this morning."},
        {15, "Another Circle battlefleet has apparently been destroyed even before it reached the Blockade. This might be going better now."},
        {16, "Tirshana, a moderately-large world in the Serris cluster, has fallen. Our forces are retreating to reinforce the Blockade."},
        {17, "The Circle has arrived at the Great Blockade today. The battles are raging right now."},
        {18, "No outstanding news this morning. The Circle forces are being held at the Blockade."},
        {19, "No news from the Blockade this morning."},
        {20, "No news this morning."},
        // WAR GOES WORSE NOW
        {21, "This morning bears the worst news since the Battle of the Arches. The Circle forces have broken through the Great Blockade, " +
             "using another void-jumping fleet. Even though half of their battlefleet has been destroyed in this dangerous jump, the surprise " +
             "was still sufficient to allow the Blockade to be perforated, even though the Circle suffered terrible casualties in the process. " +
             "One Circle battlefleet is now advancing towards Towerhold, and the Scriptorium is mustering whomever it can." +
             "\n\n" +
             "<b>Things are going to get noticeably worse now.</b>"},
        {22, "The Twelfth Fleet is apparently on its way to relieve Towerhold. They're expected tomorrow."},
        {23, "The Twelfth didn't arrive, but it's still en route and scheduled to arrive before the Circle forces. Grand " +
             "Master Chenot Chen advises calmness and courage."},
        {24, "No news this morning. The Twelfth might still make it in time."},
        {25, "It's become clear that the Twelfth Fleet won't make it before it's too late. Grand Master Chenot Chen has allegedly run away."},
        {26, "Nothing stands between the Circle and Towerhold garrison now."},
        {27, "\nThey're coming. They're expected tomorrow. All is over."},
        {28, "\n\n<b>They're here.</b>"},
        
        {29, "----- ????"},
        {30, "----- ????"},
        {31, "----- ????"},
    };

    public const int DAYS_PASSED_WAR_GOES_BAD = 12;
    public const int DAYS_PASSED_WAR_GOES_WORSE = 21;
    public const int DAYS_PASSED_FINALE = 28;

}


public static class NewGameDataCarrier {
    public static PlayerController.Talent ChosenTalent = PlayerController.Talent.None;
}