﻿using System;
using System.Linq;
using static PKHeX.Core.AutoMod.Aesthetics.PersonalColor;
using static PKHeX.Core.Ball;

namespace PKHeX.Core.AutoMod;

public static class Aesthetics
{
    private static ReadOnlySpan<PersonalColor> ShinyMap =>
    [
        Green, // Bulbasaur
        Green, // Ivysaur
        Green, // Venusaur
        Yellow, // Charmander
        Yellow, // Charmeleon
        Black, // Charizard
        Blue, // Squirtle
        Purple, // Wartortle
        Purple, // Blastoise
        Yellow, // Caterpie
        Red, // Metapod
        Purple, // Butterfree
        Yellow, // Weedle
        Green, // Kakuna
        Green, // Beedrill
        Brown, // Pidgey
        Yellow, // Pidgeotto
        Yellow, // Pidgeot
        Green, // Rattata
        Red, // Raticate
        Green, // Spearow
        Green, // Fearow
        Green, // Ekans
        Yellow, // Arbok
        Yellow, // Pikachu
        Yellow, // Raichu
        Green, // Sandshrew
        Red, // Sandslash
        Purple, // NidoranF
        Purple, // Nidorina
        Green, // Nidoqueen
        Blue, // NidoranM
        Blue, // Nidorino
        Blue, // Nidoking
        Pink, // Clefairy
        Pink, // Clefable
        Yellow, // Vulpix
        White, // Ninetales
        Pink, // Jigglypuff
        Pink, // Wigglytuff
        Green, // Zubat
        Green, // Golbat
        Green, // Oddish
        Green, // Gloom
        Green, // Vileplume
        Red, // Paras
        Red, // Parasect
        Purple, // Venonat
        Blue, // Venomoth
        Brown, // Diglett
        Brown, // Dugtrio
        White, // Meowth
        White, // Persian
        Blue, // Psyduck
        Blue, // Golduck
        Green, // Mankey
        White, // Primeape
        Yellow, // Growlithe
        Yellow, // Arcanine
        Blue, // Poliwag
        Blue, // Poliwhirl
        Green, // Poliwrath
        Yellow, // Abra
        Yellow, // Kadabra
        Yellow, // Alakazam
        Brown, // Machop
        Green, // Machoke
        Green, // Machamp
        Yellow, // Bellsprout
        Yellow, // Weepinbell
        Yellow, // Victreebel
        Blue, // Tentacool
        Blue, // Tentacruel
        Yellow, // Geodude
        Brown, // Graveler
        Brown, // Golem
        Blue, // Ponyta
        Black, // Rapidash
        Pink, // Slowpoke
        Purple, // Slowbro
        Gray, // Magnemite
        Gray, // Magneton
        Pink, // Farfetchd
        Green, // Doduo
        Green, // Dodrio
        White, // Seel
        White, // Dewgong
        Green, // Grimer
        Green, // Muk
        Red, // Shellder
        Blue, // Cloyster
        Purple, // Gastly
        Purple, // Haunter
        Purple, // Gengar
        Green, // Onix
        Pink, // Drowzee
        Pink, // Hypno
        Yellow, // Krabby
        Green, // Kingler
        Blue, // Voltorb
        Blue, // Electrode
        Yellow, // Exeggcute
        Red, // Exeggutor
        Green, // Cubone
        Green, // Marowak
        Green, // Hitmonlee
        Green, // Hitmonchan
        Yellow, // Lickitung
        Green, // Koffing
        Green, // Weezing
        Red, // Rhyhorn
        Gray, // Rhydon
        Green, // Chansey
        Green, // Tangela
        Brown, // Kangaskhan
        Blue, // Horsea
        Blue, // Seadra
        White, // Goldeen
        Red, // Seaking
        Yellow, // Staryu
        Blue, // Starmie
        White, // MrMime
        Green, // Scyther
        Pink, // Jynx
        Yellow, // Electabuzz
        Red, // Magmar
        Purple, // Pinsir
        Green, // Tauros
        Yellow, // Magikarp
        Red, // Gyarados
        Purple, // Lapras
        Blue, // Ditto
        White, // Eevee
        Purple, // Vaporeon
        Green, // Jolteon
        Yellow, // Flareon
        Pink, // Porygon
        Purple, // Omanyte
        Purple, // Omastar
        Green, // Kabuto
        Green, // Kabutops
        Purple, // Aerodactyl
        Blue, // Snorlax
        Blue, // Articuno
        Yellow, // Zapdos
        Red, // Moltres
        Pink, // Dratini
        Pink, // Dragonair
        Green, // Dragonite
        White, // Mewtwo
        Blue, // Mew
        Green, // Chikorita
        Red, // Bayleef
        Green, // Meganium
        Brown, // Cyndaquil
        Brown, // Quilava
        Brown, // Typhlosion
        Blue, // Totodile
        Blue, // Croconaw
        Blue, // Feraligatr
        Yellow, // Sentret
        Pink, // Furret
        Yellow, // Hoothoot
        Brown, // Noctowl
        Red, // Ledyba
        Red, // Ledian
        Blue, // Spinarak
        Pink, // Ariados
        Pink, // Crobat
        Blue, // Chinchou
        Purple, // Lanturn
        Yellow, // Pichu
        Pink, // Cleffa
        Pink, // Igglybuff
        White, // Togepi
        White, // Togetic
        Green, // Natu
        Green, // Xatu
        Pink, // Mareep
        Pink, // Flaaffy
        Pink, // Ampharos
        Purple, // Bellossom
        Green, // Marill
        Yellow, // Azumarill
        Brown, // Sudowoodo
        Blue, // Politoed
        Green, // Hoppip
        Pink, // Skiploom
        Red, // Jumpluff
        Red, // Aipom
        Yellow, // Sunkern
        Yellow, // Sunflora
        Blue, // Yanma
        Pink, // Wooper
        Pink, // Quagsire
        Green, // Espeon
        Black, // Umbreon
        Purple, // Murkrow
        Purple, // Slowking
        Green, // Misdreavus
        Blue, // Unown
        Pink, // Wobbuffet
        Yellow, // Girafarig
        Yellow, // Pineco
        Yellow, // Forretress
        Yellow, // Dunsparce
        Blue, // Gligar
        Yellow, // Steelix
        Purple, // Snubbull
        Pink, // Granbull
        Pink, // Qwilfish
        Green, // Scizor
        Blue, // Shuckle
        Pink, // Heracross
        Pink, // Sneasel
        Green, // Teddiursa
        Green, // Ursaring
        Gray, // Slugma
        Pink, // Magcargo
        Green, // Swinub
        Yellow, // Piloswine
        Blue, // Corsola
        Purple, // Remoraid
        Brown, // Octillery
        Purple, // Delibird
        Blue, // Mantine
        Brown, // Skarmory
        Blue, // Houndour
        Blue, // Houndoom
        Purple, // Kingdra
        Blue, // Phanpy
        Red, // Donphan
        Blue, // Porygon2
        Green, // Stantler
        Yellow, // Smeargle
        Brown, // Tyrogue
        Brown, // Hitmontop
        Pink, // Smoochum
        Yellow, // Elekid
        Red, // Magby
        Blue, // Miltank
        Pink, // Blissey
        Yellow, // Raikou
        Brown, // Entei
        Blue, // Suicune
        Green, // Larvitar
        Purple, // Pupitar
        Brown, // Tyranitar
        White, // Lugia
        Yellow, // HoOh
        Pink, // Celebi
        Green, // Treecko
        Green, // Grovyle
        Green, // Sceptile
        Yellow, // Torchic
        Yellow, // Combusken
        Red, // Blaziken
        Purple, // Mudkip
        Purple, // Marshtomp
        Purple, // Swampert
        Yellow, // Poochyena
        Yellow, // Mightyena
        Red, // Zigzagoon
        Red, // Linoone
        Purple, // Wurmple
        Yellow, // Silcoon
        Yellow, // Beautifly
        Green, // Cascoon
        Brown, // Dustox
        Red, // Lotad
        Green, // Lombre
        Yellow, // Ludicolo
        Red, // Seedot
        Red, // Nuzleaf
        Red, // Shiftry
        Red, // Taillow
        Red, // Swellow
        White, // Wingull
        Green, // Pelipper
        Blue, // Ralts
        White, // Kirlia
        White, // Gardevoir
        Black, // Surskit
        Green, // Masquerain
        Red, // Shroomish
        Red, // Breloom
        Pink, // Slakoth
        White, // Vigoroth
        Brown, // Slaking
        Yellow, // Nincada
        Yellow, // Ninjask
        Yellow, // Shedinja
        Purple, // Whismur
        Purple, // Loudred
        Purple, // Exploud
        Yellow, // Makuhita
        Purple, // Hariyama
        Green, // Azurill
        Yellow, // Nosepass
        Red, // Skitty
        Red, // Delcatty
        Yellow, // Sableye
        Black, // Mawile
        White, // Aron
        Gray, // Lairon
        Gray, // Aggron
        Red, // Meditite
        Blue, // Medicham
        Blue, // Electrike
        Black, // Manectric
        Red, // Plusle
        Green, // Minun
        Purple, // Volbeat
        Blue, // Illumise
        Green, // Roselia
        Blue, // Gulpin
        Blue, // Swalot
        Green, // Carvanha
        Purple, // Sharpedo
        Purple, // Wailmer
        Purple, // Wailord
        Yellow, // Numel
        Black, // Camerupt
        Red, // Torkoal
        Yellow, // Spoink
        Black, // Grumpig
        Green, // Spinda
        Green, // Trapinch
        Red, // Vibrava
        Green, // Flygon
        Red, // Cacnea
        Red, // Cacturne
        Yellow, // Swablu
        Yellow, // Altaria
        White, // Zangoose
        Black, // Seviper
        Yellow, // Lunatone
        Red, // Solrock
        Blue, // Barboach
        Blue, // Whiscash
        Red, // Corphish
        Red, // Crawdaunt
        Yellow, // Baltoy
        Black, // Claydol
        Green, // Lileep
        Red, // Cradily
        Brown, // Anorith
        Red, // Armaldo
        Purple, // Feebas
        Blue, // Milotic
        Purple, // Castform
        Green, // Kecleon
        Blue, // Shuppet
        Black, // Banette
        Red, // Duskull
        Red, // Dusclops
        Green, // Tropius
        Blue, // Chimecho
        Red, // Absol
        Purple, // Wynaut
        Blue, // Snorunt
        White, // Glalie
        Purple, // Spheal
        Purple, // Sealeo
        Purple, // Walrein
        Purple, // Clamperl
        Green, // Huntail
        Yellow, // Gorebyss
        Blue, // Relicanth
        Yellow, // Luvdisc
        Green, // Bagon
        Green, // Shelgon
        Green, // Salamence
        Gray, // Beldum
        Gray, // Metang
        Gray, // Metagross
        Red, // Regirock
        Blue, // Regice
        Black, // Registeel
        Yellow, // Latias
        Green, // Latios
        Purple, // Kyogre
        Green, // Groudon
        Black, // Rayquaza
        Yellow, // Jirachi
        Yellow, // Deoxys
        Blue, // Turtwig
        Blue, // Grotle
        Green, // Torterra
        Red, // Chimchar
        Red, // Monferno
        Red, // Infernape
        Blue, // Piplup
        Blue, // Prinplup
        Blue, // Empoleon
        Brown, // Starly
        Brown, // Staravia
        Brown, // Staraptor
        Yellow, // Bidoof
        Yellow, // Bibarel
        Yellow, // Kricketot
        Yellow, // Kricketune
        Yellow, // Shinx
        Yellow, // Luxio
        Black, // Luxray
        Green, // Budew
        Green, // Roserade
        Red, // Cranidos
        Red, // Rampardos
        Black, // Shieldon
        Black, // Bastiodon
        Green, // Burmy
        Green, // Wormadam
        Yellow, // Mothim
        Red, // Combee
        Red, // Vespiquen
        Pink, // Pachirisu
        Yellow, // Buizel
        Yellow, // Floatzel
        Red, // Cherubi
        Green, // Cherrim
        Blue, // Shellos
        Blue, // Gastrodon
        Pink, // Ambipom
        Yellow, // Drifloon
        Yellow, // Drifblim
        Red, // Buneary
        Red, // Lopunny
        Green, // Mismagius
        Purple, // Honchkrow
        Purple, // Glameow
        Purple, // Purugly
        Yellow, // Chingling
        Red, // Stunky
        Red, // Skuntank
        Green, // Bronzor
        Green, // Bronzong
        Brown, // Bonsly
        Pink, // MimeJr
        Pink, // Happiny
        Black, // Chatot
        Blue, // Spiritomb
        Blue, // Gible
        Blue, // Gabite
        Black, // Garchomp
        Blue, // Munchlax
        Yellow, // Riolu
        Yellow, // Lucario
        Yellow, // Hippopotas
        Yellow, // Hippowdon
        Red, // Skorupi
        Red, // Drapion
        Blue, // Croagunk
        Blue, // Toxicroak
        Green, // Carnivine
        Black, // Finneon
        Black, // Lumineon
        Blue, // Mantyke
        White, // Snover
        White, // Abomasnow
        Pink, // Weavile
        Gray, // Magnezone
        Yellow, // Lickilicky
        Yellow, // Rhyperior
        Green, // Tangrowth
        Yellow, // Electivire
        Red, // Magmortar
        White, // Togekiss
        Blue, // Yanmega
        Yellow, // Leafeon
        Blue, // Glaceon
        Blue, // Gliscor
        Brown, // Mamoswine
        Blue, // PorygonZ
        Blue, // Gallade
        Yellow, // Probopass
        Black, // Dusknoir
        White, // Froslass
        Red, // Rotom
        Yellow, // Uxie
        Red, // Mesprit
        Blue, // Azelf
        Green, // Dialga
        Pink, // Palkia
        Red, // Heatran
        Blue, // Regigigas
        Blue, // Giratina
        Purple, // Cresselia
        Blue, // Phione
        Blue, // Manaphy
        Black, // Darkrai
        Green, // Shaymin
        Yellow, // Arceus
        White, // Victini
        Green, // Snivy
        Green, // Servine
        Green, // Serperior
        Yellow, // Tepig
        Red, // Pignite
        Blue, // Emboar
        Blue, // Oshawott
        Blue, // Dewott
        Blue, // Samurott
        Brown, // Patrat
        Red, // Watchog
        Yellow, // Lillipup
        Yellow, // Herdier
        Yellow, // Stoutland
        Blue, // Purrloin
        Red, // Liepard
        Green, // Pansage
        Green, // Simisage
        Red, // Pansear
        Red, // Simisear
        Blue, // Panpour
        Blue, // Simipour
        Yellow, // Munna
        Purple, // Musharna
        Gray, // Pidove
        Green, // Tranquill
        Brown, // Unfezant
        Blue, // Blitzle
        Black, // Zebstrika
        Purple, // Roggenrola
        Purple, // Boldore
        Blue, // Gigalith
        Green, // Woobat
        Yellow, // Swoobat
        Red, // Drilbur
        Red, // Excadrill
        Purple, // Audino
        Yellow, // Timburr
        Yellow, // Gurdurr
        Red, // Conkeldurr
        Yellow, // Tympole
        Blue, // Palpitoad
        Blue, // Seismitoad
        Red, // Throh
        Blue, // Sawk
        Green, // Sewaddle
        Green, // Swadloon
        Green, // Leavanny
        Red, // Venipede
        Purple, // Whirlipede
        Red, // Scolipede
        Yellow, // Cottonee
        White, // Whimsicott
        Yellow, // Petilil
        Yellow, // Lilligant
        Green, // Basculin
        Yellow, // Sandile
        Brown, // Krokorok
        Brown, // Krookodile
        Red, // Darumaka
        Red, // Darmanitan
        Green, // Maractus
        Red, // Dwebble
        Green, // Crustle
        Yellow, // Scraggy
        Green, // Scrafty
        Black, // Sigilyph
        Blue, // Yamask
        Gray, // Cofagrigus
        Blue, // Tirtouga
        Blue, // Carracosta
        Red, // Archen
        Yellow, // Archeops
        Blue, // Trubbish
        Brown, // Garbodor
        Black, // Zorua
        Black, // Zoroark
        Red, // Minccino
        Brown, // Cinccino
        Red, // Gothita
        Black, // Gothorita
        Black, // Gothitelle
        Green, // Solosis
        Green, // Duosion
        Blue, // Reuniclus
        Pink, // Ducklett
        White, // Swanna
        White, // Vanillite
        White, // Vanillish
        White, // Vanilluxe
        Pink, // Deerling
        Yellow, // Sawsbuck
        White, // Emolga
        Green, // Karrablast
        Gray, // Escavalier
        Blue, // Foongus
        Blue, // Amoonguss
        Blue, // Frillish
        Blue, // Jellicent
        Purple, // Alomomola
        Yellow, // Joltik
        Yellow, // Galvantula
        Gray, // Ferroseed
        Yellow, // Ferrothorn
        Gray, // Klink
        Gray, // Klang
        Yellow, // Klinklang
        White, // Tynamo
        Yellow, // Eelektrik
        Green, // Eelektross
        Blue, // Elgyem
        Red, // Beheeyem
        White, // Litwick
        Black, // Lampent
        Red, // Chandelure
        Brown, // Axew
        Black, // Fraxure
        Black, // Haxorus
        White, // Cubchoo
        White, // Beartic
        Blue, // Cryogonal
        Yellow, // Shelmet
        Yellow, // Accelgor
        Yellow, // Stunfisk
        Blue, // Mienfoo
        Pink, // Mienshao
        Green, // Druddigon
        Gray, // Golett
        Gray, // Golurk
        Blue, // Pawniard
        Blue, // Bisharp
        Red, // Bouffalant
        Brown, // Rufflet
        Blue, // Braviary
        Red, // Vullaby
        Red, // Mandibuzz
        Red, // Heatmor
        Gray, // Durant
        Green, // Deino
        Green, // Zweilous
        Green, // Hydreigon
        Yellow, // Larvesta
        Yellow, // Volcarona
        Blue, // Cobalion
        Red, // Terrakion
        Red, // Virizion
        Green, // Tornadus
        Blue, // Thundurus
        White, // Reshiram
        Black, // Zekrom
        Yellow, // Landorus
        Black, // Kyurem
        Green, // Keldeo
        Green, // Meloetta
        Red, // Genesect
        Red, // Chespin
        Red, // Quilladin
        Green, // Chesnaught
        Gray, // Fennekin
        Purple, // Braixen
        Purple, // Delphox
        Blue, // Froakie
        Blue, // Frogadier
        Black, // Greninja
        Gray, // Bunnelby
        Gray, // Diggersby
        Red, // Fletchling
        Red, // Fletchinder
        Red, // Talonflame
        White, // Scatterbug
        Gray, // Spewpa
        Red, // Vivillon
        Red, // Litleo
        Red, // Pyroar
        White, // Flabébé
        White, // Floette
        Purple, // Florges
        Green, // Skiddo
        Green, // Gogoat
        Black, // Pancham
        Black, // Pangoro
        Black, // Furfrou
        Pink, // Espurr
        Yellow, // Meowstic
        Red, // Honedge
        Red, // Doublade
        Red, // Aegislash
        Purple, // Spritzee
        Purple, // Aromatisse
        Yellow, // Swirlix
        Yellow, // Slurpuff
        Brown, // Inkay
        Brown, // Malamar
        Blue, // Binacle
        Blue, // Barbaracle
        Purple, // Skrelp
        Purple, // Dragalge
        Red, // Clauncher
        Red, // Clawitzer
        Red, // Helioptile
        Red, // Heliolisk
        Blue, // Tyrunt
        Blue, // Tyrantrum
        White, // Amaura
        White, // Aurorus
        Blue, // Sylveon
        Black, // Hawlucha
        Brown, // Dedenne
        Black, // Carbink
        Yellow, // Goomy
        Yellow, // Sliggoo
        Yellow, // Goodra
        Yellow, // Klefki
        Gray, // Phantump
        Gray, // Trevenant
        Purple, // Pumpkaboo
        Purple, // Gourgeist
        Blue, // Bergmite
        Blue, // Avalugg
        Blue, // Noibat
        Blue, // Noivern
        Blue, // Xerneas
        Red, // Yveltal
        White, // Zygarde
        Pink, // Diancie
        Yellow, // Hoopa
        Yellow, // Volcanion
        Green, // Rowlet
        Green, // Dartrix
        Black, // Decidueye
        White, // Litten
        White, // Torracat
        White, // Incineroar
        Blue, // Popplio
        Blue, // Brionne
        Blue, // Primarina
        Black, // Pikipek
        Black, // Trumbeak
        Black, // Toucannon
        Brown, // Yungoos
        Brown, // Gumshoos
        Red, // Grubbin
        Red, // Charjabug
        Gray, // Vikavolt
        Purple, // Crabrawler
        White, // Crabominable
        Black, // Oricorio
        Pink, // Cutiefly
        Pink, // Ribombee
        Blue, // Rockruff
        Blue, // Lycanroc
        Blue, // Wishiwashi
        Red, // Mareanie
        Red, // Toxapex
        Yellow, // Mudbray
        Yellow, // Mudsdale
        Purple, // Dewpider
        Purple, // Araquanid
        Green, // Fomantis
        Green, // Lurantis
        Brown, // Morelull
        Brown, // Shiinotic
        White, // Salandit
        White, // Salazzle
        Yellow, // Stufful
        Yellow, // Bewear
        Red, // Bounsweet
        Purple, // Steenee
        Purple, // Tsareena
        Blue, // Comfey
        Pink, // Oranguru
        Blue, // Passimian
        Red, // Wimpod
        White, // Golisopod
        Black, // Sandygast
        Black, // Palossand
        Green, // Pyukumuku
        Brown, // TypeNull
        Yellow, // Silvally
        Black, // Minior
        Blue, // Komala
        Blue, // Turtonator
        White, // Togedemaru
        Gray, // Mimikyu
        Red, // Bruxish
        Yellow, // Drampa
        Red, // Dhelmise
        Yellow, // Jangmoo
        Green, // Hakamoo
        Green, // Kommoo
        Black, // TapuKoko
        Black, // TapuLele
        Black, // TapuBulu
        Black, // TapuFini
        Purple, // Cosmog
        Yellow, // Cosmoem
        Red, // Solgaleo
        Red, // Lunala
        Yellow, // Nihilego
        Green, // Buzzwole
        Black, // Pheromosa
        Blue, // Xurkitree
        White, // Celesteela
        White, // Kartana
        White, // Guzzlord
        Blue, // Necrozma
        Gray, // Magearna
        Black, // Marshadow
        White, // Poipole
        Yellow, // Naganadel
        Yellow, // Stakataka
        Blue, // Blacephalon
        White, // Zeraora
        Gray, // Meltan
        Gray, // Melmetal
        Green, // Grookey
        Yellow, // Thwackey
        Brown, // Rillaboom
        White, // Scorbunny
        Gray, // Raboot
        Gray, // Cinderace
        Blue, // Sobble
        Blue, // Drizzile
        Blue, // Inteleon
        Red, // Skwovet
        Red, // Greedent
        Yellow, // Rookidee
        Gray, // Corvisquire
        Gray, // Corviknight
        Blue, // Blipbug
        Blue, // Dottler
        Blue, // Orbeetle
        Brown, // Nickit
        Brown, // Thievul
        Blue, // Gossifleur
        White, // Eldegoss
        Black, // Wooloo
        Black, // Dubwool
        Green, // Chewtle
        Green, // Drednaw
        Pink, // Yamper
        Yellow, // Boltund
        Black, // Rolycoly
        Black, // Carkol
        Black, // Coalossal
        Green, // Applin
        Green, // Flapple
        Green, // Appletun
        Yellow, // Silicobra
        Black, // Sandaconda
        Red, // Cramorant
        Blue, // Arrokuda
        Blue, // Barraskewda
        Red, // Toxel
        Purple, // Toxtricity
        Red, // Sizzlipede
        Red, // Centiskorch
        Blue, // Clobbopus
        Red, // Grapploct
        Purple, // Sinistea
        Purple, // Polteageist
        White, // Hatenna
        White, // Hattrem
        White, // Hatterene
        Blue, // Impidimp
        Blue, // Morgrem
        White, // Grimmsnarl
        Red, // Obstagoon
        Yellow, // Perrserker
        Pink, // Cursola
        Yellow, // Sirfetchd
        Black, // MrRime
        White, // Runerigus
        White, // Milcery
        White, // Alcremie
        Brown, // Falinks
        Black, // Pincurchin
        White, // Snom
        White, // Frosmoth
        Black, // Stonjourner
        Purple, // Eiscue
        Black, // Indeedee
        White, // Morpeko
        Yellow, // Cufant
        Black, // Copperajah
        Brown, // Dracozolt
        White, // Arctozolt
        Brown, // Dracovish
        White, // Arctovish
        White, // Duraludon
        Green, // Dreepy
        Gray, // Drakloak
        Green, // Dragapult
        Blue, // Zacian
        Red, // Zamazenta
        Red, // Eternatus
        White, // Kubfu
        Black, // Urshifu
        Black, // Zarude
        Yellow, // Regieleki
        Red, // Regidrago
        White, // Glastrier
        Black, // Spectrier
        White, // Calyrex
        White, // Wyrdeer
        Brown, // Kleavor
        Brown, // Ursaluna
        Blue, // Basculegion
        Black, // Sneasler
        Blue, // Overqwil
        Red, // Enamorus
        Green, // Sprigatito
        Green, // Floragato
        Green, // Meowscarada
        Pink, // Fuecoco
        Pink, // Crocalor
        Pink, // Skeledirge
        Blue, // Quaxly
        Blue, // Quaxwell
        Blue, // Quaquaval
        Pink, // Lechonk
        Pink, // Oinkologne
        Yellow, // Dudunsparce
        Red, // Tarountula
        Red, // Spidops
        Yellow, // Nymble
        Green, // Lokix
        Yellow, // Rellor
        Red, // Rabsca
        Yellow, // Greavard
        Yellow, // Houndstone
        Yellow, // Flittle
        Black, // Espathra
        Red, // Farigiraf
        Yellow, // Wiglett
        Blue, // Wugtrio
        White, // Dondozo
        Green, // Veluza
        Purple, // Finizen
        Purple, // Palafin
        Green, // Smoliv
        Green, // Dolliv
        Green, // Arboliva
        Yellow, // Capsakid
        Yellow, // Scovillain
        Yellow, // Tadbulb
        Green, // Bellibolt
        Yellow, // Varoom
        Yellow, // Revavroom
        Blue, // Orthworm
        White, // Tandemaus
        White, // Maushold
        Gray, // Cetoddle
        Gray, // Cetitan
        Gray, // Frigibax
        Blue, // Arctibax
        Gray, // Baxcalibur
        Brown, // Tatsugiri
        Gray, // Cyclizar
        Red, // Pawmi
        Pink, // Pawmo
        Red, // Pawmot
        Yellow, // Wattrel
        Yellow, // Kilowattrel
        White, // Bombirdier
        Green, // Squawkabilly
        Pink, // Flamigo
        Blue, // Klawf
        Brown, // Nacli
        Brown, // Naclstack
        Brown, // Garganacl
        Blue, // Glimmet
        Blue, // Glimmora
        Black, // Shroodle
        White, // Grafaiai
        Brown, // Fidough
        Brown, // Dachsbun
        Purple, // Maschiff
        Purple, // Mabosstiff
        White, // Bramblin
        White, // Brambleghast
        Gray, // Gimmighoul
        Yellow, // Gholdengo
        Green, // GreatTusk
        Blue, // BruteBonnet
        Blue, // WalkingWake
        Black, // SandyShocks
        Pink, // ScreamTail
        Green, // FlutterMane
        Yellow, // SlitherWing
        Green, // RoaringMoon
        Gray, // IronTreads
        Gray, // IronLeaves
        Red, // IronMoth
        Gray, // IronHands
        Gray, // IronJugulis
        Gray, // IronThorns
        Gray, // IronBundle
        Gray, // IronValiant
        Red, // TingLu
        Red, // ChienPao
        Red, // WoChien
        Red, // ChiYu
        Red, // Koraidon
        Red, // Miraidon
        Pink, // Tinkatink
        Pink, // Tinkatuff
        Pink, // Tinkaton
        Red, // Charcadet
        Yellow, // Armarouge
        Black, // Ceruledge
        White, // Toedscool
        Pink, // Toedscruel
        Blue, // Kingambit
        Purple, // Clodsire
        Gray, // Annihilape
        Yellow, // Dipplin
        Black, // Poltchageist
        Black, // Sinistcha
        Black, // Okidogi
        Black, // Munkidori
        Black, // Fezandipiti
        Green, // Ogerpon
        White, // Archaludon
        Green, // Hydrapple
        Brown, // GougingFire
        Yellow, // RagingBolt
        Gray, // IronBoulder
        Blue, // IronCrown
        Blue, // Terapagos
        Purple, // Pecharunt
    ];

    public static Ball GetBallFromString(ReadOnlySpan<char> ballstr)
    {
        var space = ballstr.IndexOf(' ');
        if (space != -1)
            ballstr = ballstr[..space];

        if (ballstr is "Poké")
            return Poke;
        if (ballstr is "Feather" or "Wing" or "Jet" or "Leaden" or "Gigaton" or "Origin")
            return Parse(['L', 'A', .. ballstr]);
        return Parse(ballstr);

        static Ball Parse(ReadOnlySpan<char> tmp) => Enum.TryParse<Ball>(tmp, out var ball) ? ball : Ball.None;
    }

    public static void ApplyShinyBall(PKM pk, IEncounterTemplate enc)
    {
        var color = ShinyMap[pk.Species];
        var prefer = enc.Version == GameVersion.PLA ? GetColorsLA(color) : GetColors(color);
        ApplyFirstLegalBall(pk, enc, prefer);
    }

    private static ReadOnlySpan<Ball> LowPriority => [Poke, LAPoke];

    public static Shiny GetShinyType(ReadOnlySpan<char> value)
    {
        if (Is(value, "Square")) return Shiny.AlwaysSquare;
        if (Is(value, "Star")) return Shiny.AlwaysStar;
        if (Is(value, "Yes")) return Shiny.Always;
        if (Is(value, "No")) return Shiny.Never;
        return Shiny.Random;

        static bool Is(ReadOnlySpan<char> a, ReadOnlySpan<char> b) => a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }

    public static LanguageID? GetLanguageId(ReadOnlySpan<char> value)
    {
        var valid = Enum.TryParse(value, out LanguageID lang);
        if (!valid)
        {
            return null;
        }

        return lang is LanguageID.Hacked or LanguageID.UNUSED_6 ? LanguageID.English : lang;
    }

    /// <summary>
    /// Priority Match ball IDs that match the color ID in descending order
    /// </summary>
    private static ReadOnlySpan<Ball> GetColors(PersonalColor color) => color switch
    {
        Red => [Repeat, Fast, Heal, Great, Dream, Lure],
        Blue => [Dive, Net, Great, Lure, Beast],
        Yellow => [Level, Ultra, Repeat, Quick, Moon],
        Green => [Safari, Friend, Nest, Dusk],
        Black => [Luxury, Heavy, Ultra, Moon, Net, Beast],
        Brown => [Level, Heavy],
        Purple => [Master, Love, Heal, Dream],
        Gray => [Heavy, Premier, Luxury],
        White => [Premier, Timer, Luxury, Ultra],
        _ => [Love, Heal, Dream],
    };

    private static ReadOnlySpan<Ball> GetColorsLA(PersonalColor color) => color switch
    {
        Red => [LAPoke],
        Blue => [LAFeather, LAGreat, LAJet],
        Yellow => [LAUltra],
        Green => [LAPoke],
        Black => [LAGigaton, LALeaden, LAHeavy, LAUltra],
        Brown => [LAPoke],
        Purple => [LAPoke],
        Gray => [LAGigaton, LALeaden, LAHeavy],
        White => [LAWing, LAJet],
        _ => [LAPoke],
    };

    public static Ball ApplyFirstLegalBall(PKM pkm, IEncounterTemplate enc, ReadOnlySpan<Ball> balls)
    {
        var orig_ball = pkm.Ball;
        ulong processed = 0;
        foreach (var b in balls)
        {
            if (IsLegalWithBall(pkm, b))
                return b;
            processed |= 1UL << (byte)b;
        }

        for (byte b = 1; b < (byte)Strange; b++)
        {
            if ((processed & (1UL << b)) != 0)
                continue;
            if ((Ball)b is Poke or LAPoke)
                continue;
            if (IsLegalWithBall(pkm, (Ball)b))
                return (Ball)b;
        }
        if (IsLegalWithBall(pkm, Poke))
            return Poke;
        if (IsLegalWithBall(pkm, LAPoke))
            return LAPoke;
        return (Ball)(pkm.Ball = orig_ball);
    }

    private static bool IsLegalWithBall(PKM pkm, Ball b)
    {
        pkm.Ball = (byte)b;
        return new LegalityAnalysis(pkm).Valid;
    }

    private static bool IsDisallowed(RibbonIndex ribbon) => ribbon switch
    {
        RibbonIndex.MarkCloudy => true,
        RibbonIndex.MarkRainy => true,
        RibbonIndex.MarkStormy => true,
        RibbonIndex.MarkSnowy => true,
        RibbonIndex.MarkBlizzard => true,
        RibbonIndex.MarkDry => true,
        RibbonIndex.MarkSandstorm => true,
        _ => false,
    };

    public static bool GetRandomValidMark(this PKM pk, IBattleTemplate set, IEncounterTemplate enc, out RibbonIndex mark)
    {
        mark = 0; // throwaway value
        var markinstruction = set is RegenTemplate { Regen.HasBatchSettings: true } rt && rt.Regen.Batch.Instructions.Any(z => z.PropertyName.StartsWith("RibbonMark"));
        if (markinstruction)
            return false;

        var valid = Enumerable.Range((int)RibbonIndex.MarkLunchtime, (int)RibbonIndex.MarkSlump - (int)RibbonIndex.MarkLunchtime + 1).Where(z => !IsDisallowed((RibbonIndex)z) && MarkRules.IsEncounterMarkValid((RibbonIndex)z, pk, enc)).ToArray();

        var count = valid.Length;
        if (count == 0)
            return false;

        var randomindex = Util.Rand.Next(valid.Length);
        mark = (RibbonIndex)valid[randomindex];
        return true;
    }

    public enum PersonalColor : byte
    {
        Red,
        Blue,
        Yellow,
        Green,
        Black,

        Brown,
        Purple,
        Gray,
        White,
        Pink,
    }
}