using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using static PlayerController.AttrCheckResult;
using static PlayerController.Talent;

public class Story {
    // These two are for the context panel
    public string ContextTitle;
    public string ContextDescription;
    public Event StartingEvent;
}

public class Event {
    public Func<string> TriggerFunction;
    public Choice[] Choices;
}

public class Choice {
    public Func<string> Label;
    public Func<bool> Conditions;
    public Action Effect;
    public Event NextEvent;

    public Choice(Func<string> label, Func<bool> conditions, Action effect, Event nextEvent) {
        Label = label;
        Conditions = conditions;
        Effect = effect;
        NextEvent = nextEvent;
    }
}


// Holds all "stories". Not the best name, I know, but I'm in a hurry so sod off.
public static partial class Storyteller {
    // YES, I KNOW THIS FILE IS A BIG MESS, BUT I CAN'T BE ARSED TO MAKE A PROPER API RIGHT NOW
    private static PlayerController Player => PlayerController.Instance;
    
    // I'm sorry, this is spaghettified beyond reason now
    private static AudioController Audio => UIController.GetAudioController();


    [SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
    private static (string, int) FormattedAttrCheck(string attribute,
        (string, int) success, (string, int) critSuccess, (string, int) fail, (string, int) critFail) {
        // This is a convenience method that just prints out results properly.
        // Yes, I'm insane, I know, thank you very much.
        var check = PlayerController.AttributeCheck(attribute);
        switch (check) {
            case Success: return ($"\n<b>[{attribute} ✔]</b>\n{success.Item1}", success.Item2);
            case Fail: return ($"\n<b>[{attribute} ✘]</b>\n{fail.Item1}", fail.Item2);
            case CritSuccess: return ($"\n<b>[{attribute} ✔✔]</b>\n{critSuccess.Item1}", critSuccess.Item2);
            case CritFail: return ($"\n<b>[{attribute} ✘✘]</b>\n{critFail.Item1}", critFail.Item2);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Just a random string selector
    private static string RandStr(params string[] options) => options[PlayerController.RNG.Next(options.Length)];


    // String selector based on war effort.
    private static string WarString(string normal, string bad, string worse) {
        return Player.warState <= 0 ? normal : Player.warState == 1 ? bad : worse;
    }






    //////////////////////////////////////////// SPAWNPOINT TOOLTIP (not a real event)
    private static readonly Story ShowSpawnpointTooltip = new Story {
        ContextTitle = "WASD to move",
        ContextDescription = "E to Use; C/ESC for Character",
        StartingEvent = null
    };
    
 
    
    
    
    
    
    
    
    

    //////////////////////////////////////////// BEGGING FOR MONEY
    //
    private static readonly Event BegEvent = new Event {
        // Begging produces 2-4 creds plus 1 or 2 extra and a possibility of a snack, but costs 3 HP.
        TriggerFunction = () => {
            var warString = WarString(
                "",
                "The war effort may be taking its toll on the people, but at least some still spare a pittance for those in need.",
                "With how the war's going, it's a miracle anyone still has money left for beggars.");

            var result =
                "You spend a long while begging passers-by for charity. The occasional pedestrian drops a credit or two in your outstretched hands." +
                warString + "\n\n";

            var (checkResult, eventResult) = FormattedAttrCheck("CHA",
                ("Your charismatic nature helps things go smoothly.", 1),
                (Player.warState >= 2
                    ? "The age of free snacks appears to be over, given that people can hardly feed themselves."
                    : "A kindly soul even drops you a snack along with the coins, and you don't end up as tired.", 2),
                ($"Someone also gives you a kick in the {RandStr("face", "ribs", "chest")} for good measure.", 0),
                ("However, you get a unique learning experience: a gang of thugs has apparently decided they " +
                        "don't like you, and beat you to a pulp.", -1));

            
            // disable crit successes at war effort 2; text already changed above.
            if (Player.warState >= 2 && eventResult == 2) {
                eventResult = 1;
            }

            result += checkResult;

            // Lose 3 HP normally, 2 HP if you get a snack; and 6 if you get beaten up.
            //var lostHP = eventResult == 2 ? 2 : eventResult == -1 ? 6 : 3;
            // NOTE: THIS SEEMS TO BE TOO HEAVY A PENALTY, but reducing it a little seems to work better.
            var lostHP = eventResult == 2 ? 1 : eventResult == -1 ? 5 : 2;
            
            // And gain cash like this:
            var cashGain = PlayerController.RNG.Next(2, 5) + Mathf.Abs(eventResult) + Player.charisma / 2;
            
            
            Player.creds += cashGain;
            Player.CurHP -= lostHP;
            // gain 2 XP if you get beaten up or crit-succeed, and only 1 otherwise.
            Player.CurXP += eventResult == -1 || eventResult == 2 ? 2 : 1;
            
            if (PlayerController.PlayerDead)
                result += (eventResult == -1
                              ? " One of them appears to be a little too excitable, as he kicks you a little too hard. That ends " +
                                "up more than your battered body can take, and you never rise again."
                              : " Still, the gradual exhaustion takes its due, and you succumb to weakness.")
                          + "\n\nThis is the end of the line for you.";

            
            // for whom the bell tolls, hehe
            Audio.PlayEffect(PlayerController.PlayerDead
                ? AudioController.AudioEffect.BellNotification
                : AudioController.AudioEffect.GainCash);

  

            result +=
                $"\n\nYou make <b>{cashGain}</b> credits and <b>1</b> Experience, but lose <b>{lostHP}</b> Health.";

            return result;
        },
        Choices = new[] {
            new Choice(() => "Leave.", () => !PlayerController.PlayerDead, () => { }, null),
            new Choice(() => "Walk towards the light...", () => PlayerController.PlayerDead, () => { }, null),
        }
    };

    //
    private static readonly Story BegOnTheStreet = new Story {
        ContextTitle = "STREET",
        ContextDescription = "[E] Beg for money",
        StartingEvent = new Event {
            // Begging produces 2-4 creds plus 1 or 2 extra and a possibility of a snack, but costs 3 HP.
            TriggerFunction = () => 
                "When no other work is available, you might as well do this. The more charismatic you are, the better you should fare; " +
                "and at least, if nothing else, this can be a solid learning experience." +
                "\n\n" +
                "Begging can be dangerous, and is also exhausting. You'd better have Health above 5 to proceed safely.",
            Choices = new[] {
                new Choice(() => "Beg.", () => true, () => { }, BegEvent),
                new Choice(() => "Leave.", () => true, () => { }, null),
            }
        }
    };

    
    
    
    
    
    
    
    
    
    
    

    //////////////////////////////////////////// CONSTRUCTION SITE
    //
    private static readonly Event ConstructionWorkEvent = new Event {
        TriggerFunction = () => {
            var result =
                "The shift is long and exhausting, but you get good money at the end of it. " +
                "You notice that the stronger and tougher you are, the more work you'll be able to do, and thus the more pay you will receive.";

            var cashGain = 7 + Player.strength + Player.endurance;

            Player.creds += cashGain;
            Player.CurHP -= 8;
            Player.CurXP += 2;

            result += $"\n\nYou make <b>{cashGain}</b> credits and <b>2</b> Experience, but lose <b>8</b> Health.";
            
            Audio.PlayEffect(AudioController.AudioEffect.GainCash);

            return result;
        },
        Choices = new[] {
            new Choice(() => "Leave, and hopefully get some rest.", () => true, () => { }, null)
        }
    };

    //
    private static readonly Story ConstructionSite = new Story {
        ContextTitle = "CONSTRUCTION SITE",
        ContextDescription = "[E] Work",
        StartingEvent = new Event {
            // Construction work provides 7 + STR + END creds per 8 HP.
            // Working here costs 3 STR and 8 HP.
            TriggerFunction = () => {
                var result = WarString(
                    "The construction site appears to be the most organised place in this district.",
                    "Despite the way the war is going, the construction site is still in business.",
                    "Even though the Scriptorium is blatantly losing the war at this point, these constructors are still working.");

                if (Player.strength < 3)
                    return result + "\n\n" +
                           "You go to the site and ask the foreman if you can help. He looks at you and raises an eyebrow.\n" +
                           "'This work requires physical strength, kid. It's also incredibly taxing on the body. Come back when you're in better shape.'" +
                           "\n\n" +
                           "Working at the Construction Site requires <b>3 STR</b> and Health above 8.";
                if (Player.CurHP < 8)
                    return result + "\n\n" + "The foreman stops you with a hand outstretched.\n" +
                           "'Not right now, kid. I see you're strong enough, but I need you as healthy as possible before I let you up there.'";
                // If all set, work:
                return result + "\n\n" +
                       "You show up to the construction site, with adequate strength of body and mind to take on a shift of gruelling labour.";
            },
            Choices = new[] {
                new Choice(() => "Work!", () => Player.CurHP > 8 && Player.strength > 2, () => { }, ConstructionWorkEvent),
                new Choice(() => "Leave.", () => true, () => { }, null),
            }
        }
    };


    
    
    
    
    
    
    
    
    
    
    //////////////////////////////////////////// SUPERMARKET
    //
    private static readonly Event SupermarketTrain = new Event {
        TriggerFunction = () => {
            if (Player.charisma == 5)
                return
                    "Try as you might, it seems you've assimilated everything there was to being charismatic from the shopkeeper." +
                    " That needn't stop you from enjoying some friendly company in these trying times, though.";
            else
                return
                    "The shopkeeper's a criminal and a con-man, but you thoroughly enjoy talking to him. " +
                    "Not to mention that some of his mannerisms are outright infectious." +
                    "\n\n" +
                    "You need ■ <b>" + Player.charisma + "</b> to raise your Charisma attribute.";
        },
        Choices = new[] {
            new Choice(() => $"Improve Charisma by 1 point. (■ {Player.charisma})",
                () => Player.attrPts >= Player.charisma && Player.charisma < 5,
                () => {
                    Player.attrPts -= Player.charisma;
                    Player.charisma += 1;
                    
                    Audio.PlayEffect(AudioController.AudioEffect.BellNotification);
                }, null),
            new Choice(() => "Just talk, then leave.", () => true, () => { }, null),
        }
    };
    
    //
    private static readonly Event BuyDrugs = new Event {
        TriggerFunction = () => {
            var result = "He looks at you with a wide smile on his face.\n" +
                         "'For you, my friend, I do have something special. <i>\"Sin-Dolor\"</i>. It'll make you... well, it " +
                         "will make you into an entirely new <i>you</i>.'" +
                         "\n" +
                         "You know well what Sin-Dolor is. It's a highly illegal, yet highly powerful synthodrug.\n" +
                         "<b>Using Sin-Dolor grants you 1 Attribute point, but costs 14 Health and money.</b>" +
                         "\n\n";

            result += WarString("The shopkeeper smiles. 'For you, old pal, one shot of Sin-Dolor for 40 creds.'",
                "The shopkeeper smiles. 'For you, old pal, one shot of Sin-Dolor for 50 creds. The good stuff is getting " +
                "incredibly difficult to get a hold of, what with the war and so on.'",
                "The shopkeeper smiles, but his smile is strained. 'For you, old pal, the lowest price I can ask for" +
                "a shot of Sin-Dolor is 80 creds. It's virtually impossible to find, what with the Circle on our doorstep.'");

            return result;
        },
        Choices = new[] {
            new Choice(() => {
                    var price = Player.warState == 0 ? 40 : Player.warState == 1 ? 50 : 80;
                    return $"Buy a shot of the drug. (֎ {price}, +1 ■)";
                },
                () => Player.CurHP >= 15 && (Player.warState == 0 && Player.creds >= 40 ||
                      Player.warState == 1 && Player.creds >= 50 ||
                      Player.warState == 2 && Player.creds >= 80),
                () => {
                    var price = Player.warState == 0 ? 40 : Player.warState == 1 ? 50 : 80;
                    Player.creds -= price;
                    Player.attrPts += 1;
                    Player.CurHP -= 14;
                    
                    Audio.PlayEffect(AudioController.AudioEffect.BellNotification);
                }, null),
            new Choice(() => "Shake your head and leave. No drugs right now.", () => Player.CurHP >= 15, () => { },
                null),
            new Choice(() => "Shake your head. You're not feeling tough enough to use it yet.", () => Player.CurHP < 15,
                () => { }, null),
            new Choice(() => "Shake your head. You're not rich enough to buy any yet.",
                () => Player.warState == 0 && Player.creds < 40 ||
                      Player.warState == 1 && Player.creds < 50 ||
                      Player.warState == 2 && Player.creds < 80, () => { }, null),
        }
    };

    //
    private static readonly Story Supermarket = new Story {
        ContextTitle = "SUPERMARKET",
        ContextDescription = "[E] Enter",
        StartingEvent = new Event {
            TriggerFunction = () => {
                var result = WarString(
                    "About half of the shelves of this supermarket appear to be empty. The war is taking its toll on everyone.",
                    "The supermarket has seen better days. Aside from basic goods, almost everything else appears to be gone.",
                    "The situation here is dire. Virtually all shelves are empty, long cleaned out by the desperate civilians.");
                result +=
                    " There is nothing for you here, but you only come to strike up conversation with the charismatic, yet oddly sly shopkeeper.";
                return result;
            },
            Choices = new[] {
                new Choice(() => "Converse with the shopkeeper. <b>[CHA trainer]</b>", () => true, () => { },
                    SupermarketTrain),
                new Choice(() => "Ask him about the 'special offer'.", () => true, () => { },
                    BuyDrugs),
                new Choice(() => "Leave.", () => true, () => { }, null),
            }
        }
    };

    
    
    
    
    
    
    
    
    
    
    
    

    //////////////////////////////////////////// BANK
    //
    private static readonly Event BankTrain = new Event {
        TriggerFunction = () => {
            if (Player.strength == 5)
                return
                    "Even the old powerlifter can't teach you anything more. You seem to have hit " +
                    "a plateau with your strength training, at least for the moment.";
            return
                "The security guard used to be a powerlifter back in the day, an art that predates Towerhold itself by a decamillenium. " +
                "He always seems able to show you tricks you never knew existed." +
                "\n\n" +
                "You need ■ <b>" + Player.strength + "</b> to raise your Strength attribute.";
        },
        Choices = new[] {
            new Choice(() => $"Improve Strength by 1 point. (■ {Player.strength})",
                () => Player.attrPts >= Player.strength && Player.strength < 5,
                () => {
                    Player.attrPts -= Player.strength;
                    Player.strength += 1;
                    
                    Audio.PlayEffect(AudioController.AudioEffect.BellNotification);
                }, null),
            new Choice(() => "Converse and depart.", () => true, () => { }, null),
        }
    };

    //
    private static readonly Story Bank = new Story {
        ContextTitle = "BANK",
        ContextDescription = "[E] Enter",
        StartingEvent = new Event {
            TriggerFunction = () => {
                var result = WarString(
                    "Bank services are generally denied to private citizens because of the war effort, but the bank seems populated. The security guard is here, which",
                    "The bank looks like not a single civilian enters. Aside from a single gaunt cashier lady, the security guard remains present. This",
                    "Even the staff seems to have abandoned the bank now, except for the security guard who still protects the premises. That");
                result +=
                    " is fortunate, since you only come here to speak to him. This burly man is one of the strongest people you've ever seen, " +
                    "and he's also one of the keenest when it comes to helping others improve their strength.";
                return result;
            },
            Choices = new[] {
                new Choice(() => "Ask the guard for lifting tips. <b>[STR trainer]</b>", () => true, () => { }, BankTrain),
                new Choice(() => "Leave.", () => true, () => { }, null),
            }
        }
    };

    
    
    
    
    
    
    
    
    

    //////////////////////////////////////////// OLD VETERAN
    //
    private static readonly Event VeteranTrain = new Event {
        TriggerFunction = () => {
            if (Player.endurance == 5)
                return
                    "It seems you've heard all the stories from this old tactician now, and can glean no more wisdom from " +
                    "these tales of courage and tenacity in the face of danger. Still, you can keep the old man company.";
            return
                "The old soldier's tales are enthralling and full with wisdom to the brim. As a commander of men and spacecraft, " +
                "he has seen more of war than you ever shall, there is much that you can learn from him. Indeed, by thinking about " +
                "the situations he's been in, your own toils seem almost insignificant." +
                "\n\n" +
                "You need ■ <b>" + Player.endurance + "</b> to raise your Endurance attribute.";
        },
        Choices = new[] {
            new Choice(() => $"Improve Endurance by 1 point. (■ {Player.endurance})",
                () => Player.attrPts >= Player.endurance && Player.endurance < 5,
                () => {
                    Player.attrPts -= Player.endurance;
                    Player.endurance += 1;
                    
                    Audio.PlayEffect(AudioController.AudioEffect.BellNotification);
                }, null),
            new Choice(() => "Speak to the old soldier, then go about your way.", () => true, () => { }, null),
        }
    };

    //
    private static readonly Story OldVeteran = new Story {
        ContextTitle = "OLD VETERAN",
        ContextDescription = "[E] Talk",
        StartingEvent = new Event {
            TriggerFunction = () => {
                var result =
                    "You approach the old veteran who hangs around the corner of the army office and the bank. He's told you he used to be an Adjudicator, " +
                    "in the Forty-Fourth Winged Brigade, a commander of scores and scores of men and their machines from one of Towerhold's most prestigious " +
                    "armies. The Forty-Fourth Brigade has been in service of the Scriptorium since its inception, and their resolve has only doubled after the " +
                    "relocation of the capital here in Towerhold." +
                    "\n\n" +
                    "That aside, the old Adjudicator's career has been equally as impressive. He only went in retirement shortly after the loss of Highspire, " +
                    "and shortly before the Scriptorium's next humiliating defeat -- and loss of the entire Third Fleet -- at the Battle of the Arches." +
                    "\n\n";

                result += WarString(
                    "The old soldier grins as you approach, his ever-present tobacco and Tkhel bark cigarette between his lips.",
                    "The old soldier looks more cadaverous than ever, but still gives you a friendly nod. The news of the continuing defeats " +
                    "must be affecting him worse than most other civilians. He puffs on his cigarette as you approach.",
                    "The old man seems to be at the edge of his strength, perhaps his endurance and tenacity has finally been spent. Perhaps most worryingly, " +
                    "his signature cigarette is missing.");
                return result;
            },
            Choices = new[] {
                new Choice(() => "Ask the veteran for a war story. <b>[END trainer]</b>", () => true, () => { },
                    VeteranTrain),
                new Choice(() => "Leave.", () => true, () => { }, null),
            }
        }
    };


    
    
    
    
    
    
    
    
    
    
    
    //////////////////////////////////////////// ARMY DRAFTING
    //
    private static readonly Event ArmyNeedEndurance = new Event {
        TriggerFunction = () => WarString(
                                    "The squadsman shakes his head.\n" +
                                    "'Basic training is brutal, kid, especially in a time of war. You have to be tougher to make it.'",
                                    "The squadsman shakes his head.\n" +
                                    "'Basic training is brutal, kid, and you'll be deployed to front lines afterwards. You need to be tougher to make it.'",
                                    "The squadsman grins at you.\n" +
                                    "'Even though we're all going to die, and even though you should be lucky you're alive instead of trying to join the army " +
                                    "and thus kill yourself, I'd take you. Still, you'd need to be put through basic training, and I see you wouldn't make it.'")
                                + "\n\nEnlisting with the Army requires <b>3 END</b>.",
        Choices = new[] {
            new Choice(() => "Leave.", () => true, () => { }, null),
        }
    };

    //
    private static readonly Event ArmyJoinInfantry = new Event {
        TriggerFunction = () => {
            var result =
                "You accept the offer and join the Infantry. You are taken off-world on the same day and watch Towerhold, " +
                "a world you've never left before, shrink in the window as your glider heads for Station 34." +
                "\n\n" +
                "Basic training is indeed brutal, but you make it. ";

            result += WarString(
                "Shortly after that, you and your squad of equally-green recruits " +
                "are dropped off to defend Tirshana, a moderately-large world in the Serris cluster. About a week " +
                "passes peacefully before the forces of the Circle arrive. When your commanders refuse to surrender, " +
                "the Circle begins orbital bombardment. Your entire division, as well as a quarter of the " +
                "planet's population -- about a hundred million -- are killed in the first day. "
                + (Player.talent == Marksman
                    ? "However, being a marksman, you are on patrol outside of the major city-centers. You survive, " +
                      "blending in with the civilian population as the Circle occupies the planet."
                    : "You fare no better.")
                ,
                "Shortly after that, you and your squad of equally-green recruits " +
                "are sent to defend Tirshana, a moderately-large world in the Serris cluster. You never make there. " +
                "Your carrier group is intercepted along the way by a Circle battlefleet. Your ship is unable to flee " +
                "on time, and you, along with half of your division are killed along with it."
                ,
                "However, the Circle is upon you before you even have time to be deployed. A void-jumping Circle " +
                "battlefleet surrounds Station 34. You die repelling Circle boarders, or during the bombardment. It matters not.");

            result += "\n\nThis is the end of the line for you.";

            Audio.PlayEffect(AudioController.AudioEffect.BellNotification);
            
            // scripted death :( - or not really! but game over anyway
            Player.wasSoldier = true;
            if (Player.warState == 0 && Player.talent == Marksman) Player.livedAtTheEnd = true;
            Player.CurHP = 0;

            return result;
        },
        Choices = new[] {
            new Choice(() => "Walk towards the light...", () => !Player.livedAtTheEnd, () => { }, null),
            new Choice(() => "Proceed.", () => Player.livedAtTheEnd, () => { }, null),
        }
    };

    //
    private static readonly Event ArmyPilot = new Event {
        TriggerFunction = () => {
            var result =
                "You accept the offer and join the Army as a pilot. You are taken off-world on the same day and watch Towerhold, " +
                "a world you've never left before, shrink in the window as your glider heads for Station 34." +
                "\n\n" +
                "Basic training is indeed brutal, but you make it; and you achieve your dream of becoming a pilot. You are assigned " +
                "to the Eleventh Wing of the Seventy-Third Winged Brigade, consisting almost exclusively of recruits. " +
                $"The old veteran would be proud of you if he were here{(Player.talent == Pilot ? ", as would your dad." : ".")}" +
                "\n\n" +
                "Shortly after that, you and your wing are sent to space. ";

            result += WarString(
                "You participate in numerous glorious battles and take down a score of foes yourself, but the gliders " +
                "of the Circle are endless. You die in a blaze of glory, defending your carrier group from Circle cruisers in the " +
                "sky above Tirshana, a moderately-large world in the Serris cluster. Your death has given thousands of civilians " +
                "time to evacuate."
                ,
                "You wing is sent to defend Tirshana, a moderately-large world in the Serris cluster, but you arrive too late. " +
                "You retreat, but the enemy is unstoppable, and after numerous of battles, you are killed trying to halt the enemy's " +
                "approach towards Towerhold itself."
                ,
                "Shortly after that, you and your wing take to space. By this point the enemy is upon Towerhold itself, " +
                "and you are sent as a relief force. You arrive too late, and engage the enemy battlefleet after the bombardment " +
                "has stopped. The enemy is insurmountable and you are knocked out of the sky, sent barrelling down towards your homeworld. "
                + (Player.talent == Pilot
                    ? "You miraculously manage to eject on time, as if guided by the hand of your long-deceased father " +
                      "-- like the sort of nonsense the Circle believes is real. Still, you survive the landing."
                    : "You fail to eject on time, trying to save the craft, and are killed as you crash into solid ground.")
                );

            result += "\n\nThis is the end of the line for you.";

            // scripted death :( - or not really! but game over anyway
            Player.wasPilot = true;
            if (Player.warState == 2 && Player.talent == Pilot) Player.livedAtTheEnd = true;
            Player.CurHP = 0;
            
            Audio.PlayEffect(AudioController.AudioEffect.BellNotification);

            return result;
        },
        Choices = new[] {
            new Choice(() => "Walk towards the light...", () => !Player.livedAtTheEnd, () => { }, null),
            new Choice(() => "Proceed.", () => Player.livedAtTheEnd, () => { }, null),
        }
    };

    //
    private static readonly Event ArmyBribe = new Event {
        TriggerFunction = () => {
            var isPilot = Player.talent == Pilot;

            const string s1 =
                "The squadsman looks at you. 'Maybe you should fly then. For your father's name. And we " +
                "might just have enough time to train you.'";
            const string s2 =
                "The squadsman frowns. 'You deserve to be thrown out of this office for suggesting this. " +
                "We have to start losing the war before I'd even consider corruption. Out. NOW!'";

            var result = WarString(
                $"{(isPilot ? s1 : s2)}", // cause it gets too messy otherwise
                $"The squadsman looks pensive for a moment. 'The times have gone dire. " +
                $"{(isPilot ? "Fifty" : "One hundred")} credits, and you're on board.'",
                $"The squadsman crosses his arms. 'The world is going to shit. " +
                $"{(isPilot ? "One" : "Two")} hundred credits, and you can do whatever you want.'");

            if (isPilot && Player.warState > 0) {
                result += "\nYou give him a look.\n'What,' he says. 'Normally the prices are twice as high." +
                          " You're getting a discount because of your dad.'";
            }

            if (Player.warState == 2)
                result +=
                    "\nHe shrugs. 'I need the money to get my family out of here before it's too late'. We need just a little bit more.'";

            return result;
        },
        Choices = new[] {
            // WAR STATE 0, PILOT
            new Choice(() => "Accept the offer and become a pilot!",
                () => Player.warState == 0 && Player.talent == Pilot, () => { }, ArmyPilot),
            // WAR STATE 1, half price for pilots
            new Choice(() => "Give him the money (֎ 50)",
                () => Player.warState == 1 && Player.talent == Pilot && Player.creds >= 50,
                () => Player.creds -= 50, ArmyPilot),
            // WAR STATE 1 NORMAL
            new Choice(() => "Give him the money (֎ 100)",
                () => Player.warState == 1 && Player.talent != Pilot && Player.creds >= 100,
                () => Player.creds -= 100, ArmyPilot),
            // WAR STATE 2, half price for pilots (actually same as above, lol)
            new Choice(() => "Give him the money (֎ 100)",
                () => Player.warState == 2 && Player.talent == Pilot && Player.creds >= 100,
                () => Player.creds -= 100, ArmyPilot),
            // WAR STATE 2 NORMAL
            new Choice(() => "Give him the money (֎ 200)",
                () => Player.warState == 2 && Player.talent != Pilot && Player.creds >= 200,
                () => Player.creds -= 200, ArmyPilot),
            // ANY WAR STATE, LEAVE
            new Choice(() => "Shake your head and leave.", () => true, () => { }, null),
        }
    };


    //
    private static readonly Event ArmyEnlist = new Event {
        TriggerFunction = () => {
            var result =
                "The squadsman nods. 'You look tough enough to make it through basic training. All right. But piloting...'\n\n" +
                "You ask what if there's a problem with that.\n";
            result += WarString(
                          "'Pilots are officers, bannermen and higher ranks. We can enlist you as a linesman in the Infantry though.'"
                          ,
                          "'We're in need of trained pilots, not rookies. I doubt we'd be able to train you in time, even if we wanted. " +
                          "Still, we can enlist you as a linesman in the Infantry. Solid work, and necessary. "
                          ,
                          "'Kid, we're losing the war. We're in desperate need of vehicles and trained pilots, not cannon fodder for " +
                          "the Circle's forces. Besides, pilots have a life expectancy of hours nowadays... You could join the Infantry if you want. " +
                          "It's probably safer for you to do so, anyway, than being a pilot.")
                      // special note if the player's a marksman
                      + (Player.talent == Marksman
                          ? " Not to mention that an accomplished MARKSMAN like you could go far in the Infantry!'"
                          : "'")
                      + "\n\nThe fate of a linesman in the infantry is usually to be slaughtered in some way or another, but it is an option you could take...";
            return result;
        },
        Choices = new[] {
            new Choice(() => "Accept the offer to join the Infantry.", () => true, () => { }, ArmyJoinInfantry),
            new Choice(() => "[PILOT DAD] Remark that your dad died a hero when Highspire fell.",
                () => Player.talent == Pilot, () => { }, ArmyBribe),
            new Choice(() => "(Bribe) Suggest that you could compensate him with credits.",
                () => Player.talent != Pilot, () => { }, ArmyBribe),
            new Choice(() => "Shake your head and leave.", () => true, () => { }, null),
        }
    };


    //
    private static readonly Story ArmyOffice = new Story {
        ContextTitle = "ARMY OFFICE",
        ContextDescription = "[E] Enter",
        StartingEvent = new Event {
            TriggerFunction = () => {
                var result =
                    "Your only real hope of becoming a pilot is to be achieved through the Army Office, where you might " +
                    "enlist and serve in one of Towerhold's Winged Brigades as part of a Fleet." +
                    "\n\n";

                result += WarString(
                    "As you enter, the squadsman, in his immaculate uniform, raises his eyes from the papers on the " +
                    "desk. He gives you a long, stern look, then invites you in and asks you whether you're interested in joining the Army.",
                    "The squadsman at the desk is looking grim, and his uniform isn't as crisp as it was before the war took a turn " +
                    "for the worst. Still, his composure appears intact, and he invites you in, inquiring what you seek.",
                    "The squadsman appears to have abandoned all hope, and to have turned to the bottle. He grins when you enter, all " +
                    "signs of prior discipline nowhere to be seen. With a grim laugh, he asks what you could possibly want.");
                return result;
            },
            Choices = new[] {
                new Choice(() => "Ask to enlist and become a pilot.", () => Player.endurance < 3, () => { },
                    ArmyNeedEndurance),
                new Choice(() => "Ask to enlist and become a pilot.", () => Player.endurance >= 3, () => { }, ArmyEnlist),
                new Choice(() => "Leave.", () => true, () => { }, null),
            }
        }
    };
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
       
    
    
    
        
    //////////////////////////////////////////// SLEEPING: NEWS AND WAR STATE PROGRESSION
    private static readonly Event FinaleEvent = new Event {
        TriggerFunction = () => {
            var result = "The Circle is here, and they're here to stay. Every able-bodied citizen is being drafted into an " +
                         "alleged militia. Although the Circle loses thousands of men and spacecraft in the battle, the " +
                         "Towerhold garrison is unable to stop them." +
                         "\n\n" +
                         "Your militia ends up engaging the tall, well-armoured Circle soldiers. Even in battle they go wreathed " +
                         "in branches of little-known trees, allegedly relying on their so-called \"spirits\" to guide them. Your " +
                         "group manages to take out a squad of Circle warriors, even two... but the odds are not in your favour, as " +
                         "scores of attackers keep disembarking from the fat transport vessels the Circle so much likes to use." +
                         "\n\n";
            if (Player.talent == Marksman) result += "Your marksmanship skills prove useful, as your well-placed lasgun shots take out " +
                                                     "a few of the invaders, but you alone would be like trying to stem the tide of the " +
                                                     "sea with a stick.";
            if (Player.talent == Witty)
                result += "In this time of dire need, your wit betrays you and you find yourself out of things to say.";

            result += "\n\n" +
                      "You and the people around you delay the inevitable for as long as possible, but fail to stop it, like so many " +
                      "others before you. You are killed on the streets of your city.";

            if (Player.talent == Pilot) {
                result += " And even though you didn't become a pilot, you feel your father would be proud of your valour.";
            }
            
            Audio.PlayEffect(AudioController.AudioEffect.BellNotification);
            
            result += "\n\nThis is the end of the line for you.";
            
            Player.CurHP = 0;
            Player.wasDraftedLate = true;

            return result;
        },
        Choices = new[] {
            new Choice(() => $"Walk towards the light...",() => true, () => { }, null)
        }
    };
    
    private static readonly Event SleepWakeUpNews = new Event {
        TriggerFunction = () => {
            // This happens AFTER the day has been incremented, so we can just check that.
            var currentNews = DateStuff.DailyNews[Player.numDaysPassed];

            if (Player.numDaysPassed < DateStuff.DAYS_PASSED_WAR_GOES_BAD) Player.warState = 0;
            else if (Player.numDaysPassed < DateStuff.DAYS_PASSED_WAR_GOES_WORSE) Player.warState = 1;
            else if (Player.numDaysPassed < DateStuff.DAYS_PASSED_FINALE) Player.warState = 2;

            var result = (Player.numDaysPassed <= DateStuff.DAYS_PASSED_WAR_GOES_WORSE
                             ? "You gather the latest goings-on.\n\n"
                             : "") + "<i>" + currentNews + "</i>";
            
            // play the bell noise on the "important days", otherwise the paper crumpling
            var redLetterDay = Player.numDaysPassed == DateStuff.DAYS_PASSED_WAR_GOES_BAD ||
                               Player.numDaysPassed == DateStuff.DAYS_PASSED_WAR_GOES_WORSE ||
                               Player.numDaysPassed == DateStuff.DAYS_PASSED_FINALE;
            Audio.PlayEffect(redLetterDay
                ? AudioController.AudioEffect.BellNotification
                : AudioController.AudioEffect.ReadNewsNoNotif);
            

            return result;
        },
        Choices = new[] {
            new Choice(() => $"Proceed...",() => Player.numDaysPassed < DateStuff.DAYS_PASSED_FINALE, () => { }, null),
            new Choice(() => $"Face the end.",() => Player.numDaysPassed == DateStuff.DAYS_PASSED_FINALE, () => { }, FinaleEvent)
        }
    };
    
    

    
    
    
    
    

    
    
    
    
    
    
    

    //////////////////////////////////////////// SLEEPING UNDER THE BRIDGE
    private static readonly Event SleepUnderBridgeEvent = new Event {
        // Sleeping under the bridge restores 2-4 HP + END check for 0-3 extra [total 0-6 possible]
        TriggerFunction = () => {
            var check = PlayerController.AttributeCheck("END");
            var result = "You spend the night under the bridge, as you're so used to doing.";
            var (checkResult, rollBonus) = FormattedAttrCheck("END",
                ("You managed to sleep decently.", 1),
                ("Somehow, this night was remarkably refreshing.", 3),
                ("Your sleep is disturbed during the night, but this is par for the course.", 0),
                ("Rest is hard to come by in this misery, and you wake up unrefreshed.", 0));
            var roll = PlayerController.RNG.Next(2, 5) + rollBonus;
            result += checkResult + "\n\nYou recover <b>" + roll + "</b> Health, and it is now tomorrow.";
            Player.CurHP += roll;
            Player.numDaysPassed += 1;
            
            return result;
        },
        Choices = new[] {
            new Choice(() => $"Wake up and smell the{WarString("... garbage", "... garbage", " ashes")}.",
                () => true, () => { }, SleepWakeUpNews)
        }
    };

    private static readonly Story SleepUnderTheBridge = new Story {
        ContextTitle = "BRIDGE",
        ContextDescription = "[E] Sleep under",
        StartingEvent = new Event {
            // Sleeping under the bridge restores 2-4 HP + END check for 0-3 extra [total 0-6 possible]
            TriggerFunction = () => 
                "When no other shelter is available, you can always sleep under the bridge with the other homeless beggars." +
                "\n\n" +
                "Sleeping under the bridge is rather miserable and hardly very refreshing, and is best avoided if possible.",
            Choices = new[] {
                new Choice(() => "Sleep under the bridge.", () => true,() => { }, SleepUnderBridgeEvent),
                new Choice(() => "Decide against resting right now.", () => true, () => { }, null),
            }
        }
    };














    //////////////////////////////////////////// SLEEPING IN THE HOTEL
    private static readonly Event SleepInsideEvent = new Event {
        // Sleeping inside recovers 10 + 0-5 HP.
        TriggerFunction = () => {
            var check = PlayerController.AttributeCheck("END");
            var result = "You rent a room for the night.";
            var (checkResult, rollBonus) = FormattedAttrCheck("END",
                ("You sleep soundly through the night.", 2),
                ("On the morrow, you feel spectacularly refreshed.", 5),
                ("Whilst the lodgings are adequate, sleep doesn't come to you almost until dawn.", 0),
                ("Whilst it's better than the street, you thrash and turn through the night, and sleep comes late.", 0));
            var roll = 10 + rollBonus;
            result += checkResult + "\n\nYou recover <b>" + roll + "</b> Health, and it is now tomorrow.";
            Player.CurHP += roll;
            Player.numDaysPassed += 1;

            return result;
        },
        Choices = new[] {
            new Choice(() => $"Wake up and smell the{WarString(" coffee", "... coffee", " ashes")}.",
                () => true, () => { }, SleepWakeUpNews)
        }
    };

    //
    private static readonly Story SleepInside = new Story {
        ContextTitle = "HOTEL",
        ContextDescription = "[E] Sleep comfortably",
        StartingEvent = new Event {
            // Sleeping inside recovers 10 + 0-5 HP.
            TriggerFunction = () => {
                var result = WarString(
                    "You saunter up to the reception and ask for a room. The thuggish-looking proprietor gives " +
                    "you a glance.\n'Ten creds.'",
                    "The thuggish proprietor looks antsy as he sees you enter. 'Half my tenants have left this dump " +
                    "already, kid. 'Cause of the war. Rooms are available for 5 creds a night.'",
                    "The proprietor is nowhere to be seen. He's probably run away like virtually every other tenant. " +
                    "You can choose any room you like for free.");
                result += Player.creds >= (Player.warState == 0 ? 10 : Player.warState == 1 ? 5 : 0)
                    ? ""
                    : "\nYou realise you can't afford a room here right now.'";
                return result;
            },
            Choices = new[] {
                new Choice(() => {
                        var price = (Player.warState == 0 ? 10 : Player.warState == 1 ? 5 : 0);
                        return Player.warState == 2
                            ? "Take a room for the night."
                            : $"Rent a room for the night. (֎ {price})";
                    },
                    () => Player.creds >= (Player.warState == 0 ? 10 : Player.warState == 1 ? 5 : 0),
                    () => Player.creds -= (Player.warState == 0 ? 10 : Player.warState == 1 ? 5 : 0), SleepInsideEvent),
                new Choice(() => "Decide against resting right now.",
                    () => Player.creds >= (Player.warState == 0 ? 10 : Player.warState == 1 ? 5 : 0), () => { }, null),
                new Choice(() => "Leave. You're too poor to sleep here.",
                    () => Player.creds < (Player.warState == 0 ? 10 : Player.warState == 1 ? 5 : 0), () => { }, null),
            }
        }
    };

    
    
    
    
    
    
    


    // THIS IS IMPORTANT, DON'T FORGET TO UPDATE IT
    public static readonly Dictionary<string, Story> Storybook = new Dictionary<string, Story> {
        {"ShowSpawnpointTooltip", ShowSpawnpointTooltip},
        {"BridgeSleep", SleepUnderTheBridge},
        {"HotelSleep", SleepInside},
        {"Beg", BegOnTheStreet},
        {"ConstructionSite", ConstructionSite},
        {"Supermarket", Supermarket},
        {"Bank", Bank},
        {"ArmyOffice", ArmyOffice},
        {"OldVeteran", OldVeteran},
    };
}