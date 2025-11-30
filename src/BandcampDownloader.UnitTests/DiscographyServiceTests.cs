using BandcampDownloader.Bandcamp.Extraction;
using BandcampDownloader.UnitTests.Resources;

namespace BandcampDownloader.UnitTests;

public sealed class DiscographyServiceTests
{
    private DiscographyService _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new DiscographyService();
    }

    [Test]
    public void GetRelativeAlbumsUrl_Returns_Expected_Given_AffektrecordingsHtml()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.AffektrecordingsHtml);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetRelativeAlbumsUrlsFromArtistPage(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(83));
            Assert.That(albumsUrls, Does.Contain("/album/afk057-perverse-act-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd06-concrete-tools-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd05-ciclo-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk056-illogical-method-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk055-blue-morning-breath-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd04-onda-portante-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk053-glimmer-of-light-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk052-wandering-derelicts-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk051-xiera-brief-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk050-moonlight-eruption-lp"));
            Assert.That(albumsUrls, Does.Contain("/album/afk049-under-the-lights-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk048-an-inevitable-consequence-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkrw03-classic-reworks-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk047-trigonom-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk046-zyklon-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk045-dimensions-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkrw02-fault-system-reworks-pt-2"));
            Assert.That(albumsUrls, Does.Contain("/album/afkrw01-fault-system-reworks-pt-1"));
            Assert.That(albumsUrls, Does.Contain("/album/afk044-murder-in-affekt-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk043-fault-system-lp"));
            Assert.That(albumsUrls, Does.Contain("/album/afk042-perpetual-spirit-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkva04-various-artist-04"));
            Assert.That(albumsUrls, Does.Contain("/album/afk040-mandala-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk039-frozen-lake-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkva03-various-artist-03"));
            Assert.That(albumsUrls, Does.Contain("/album/afkva02-various-artist-02"));
            Assert.That(albumsUrls, Does.Contain("/album/afk037-loss-of-sense-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk036-motion-shape-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkva01-various-artist-01"));
            Assert.That(albumsUrls, Does.Contain("/album/afk035-the-mist-advances-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk034-crash-test-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk033-her-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/afk032-galaxy-x-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk031-random-poison-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd004-her-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk029-exotic-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk028-equality-for-her-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk27-multiple-faces-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk26-skimming-the-fog-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk025-walls-and-waepons-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk024-mn80-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk023-id33"));
            Assert.That(albumsUrls, Does.Contain("/album/afk022-differenz-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd003-calypso-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk021-night-shift-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk020-raw-plant-ep-incl-vsk-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/afk019-i-want-to-believe-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk018-sonorus-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk017-edunsa-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk016-the-theban-cycle-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk015-equinox-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd002-oman-lp"));
            Assert.That(albumsUrls, Does.Contain("/album/afk014-reflection-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd01-nefesis-remixes"));
            Assert.That(albumsUrls, Does.Contain("/album/afk013-cmtrn-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk012-rugged-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk011-various-artists-vol-1"));
            Assert.That(albumsUrls, Does.Contain("/album/afk010-sden-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk009-forced-evacuation"));
            Assert.That(albumsUrls, Does.Contain("/album/afk008-road-to-nowhere-mtd-special-remixes"));
            Assert.That(albumsUrls, Does.Contain("/album/afk007-nefesis"));
            Assert.That(albumsUrls, Does.Contain("/album/afk006-prototype"));
            Assert.That(albumsUrls, Does.Contain("/album/afk005-a-light-in-the-dark"));
            Assert.That(albumsUrls, Does.Contain("/album/afk004-road-to-nowhere-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk003-dt102"));
            Assert.That(albumsUrls, Does.Contain("/album/afk002-viral-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk001-modular-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk069-10-years-affekt-recordings-lp"));
            Assert.That(albumsUrls, Does.Contain("/album/afk068-indymedia-001-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk067-vortice-di-profondit-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk066-panorama-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk065-the-turn-of-the-screw-lp"));
            Assert.That(albumsUrls, Does.Contain("/album/afk064-the-brave-unicorns"));
            Assert.That(albumsUrls, Does.Contain("/album/afk063-esperanza-eviratio-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk062-orbital-searches-ep-2"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd010-the-dwelling-place-rework"));
            Assert.That(albumsUrls, Does.Contain("/album/afk061-circle-of-life-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkmx03-dynamic-prospective-vol-1"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd09-the-distance-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk059-the-dwelling-place-lp"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd08-the-dwelling-place-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afkltd07-transmitters-and-receivers-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/afk058-x-pression-ep"));
        }
    }

    [Test]
    public void GetRelativeAlbumsUrl_Returns_Expected_Given_ProjectmooncircleHtml()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.ProjectmooncircleHtml);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetRelativeAlbumsUrlsFromArtistPage(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(120));
            Assert.That(albumsUrls, Does.Contain("/album/wild"));
            Assert.That(albumsUrls, Does.Contain("/album/laws-of-nature"));
            Assert.That(albumsUrls, Does.Contain("/album/mood-in-c"));
            Assert.That(albumsUrls, Does.Contain("/album/works"));
            Assert.That(albumsUrls, Does.Contain("/album/pretty-dark-things"));
            Assert.That(albumsUrls, Does.Contain("/album/the-aura"));
            Assert.That(albumsUrls, Does.Contain("/album/while-it-still-blooms"));
            Assert.That(albumsUrls, Does.Contain("/album/there-be-monsters"));
            Assert.That(albumsUrls, Does.Contain("/album/drafts-lost-tracks-2010-2014"));
            Assert.That(albumsUrls, Does.Contain("/album/darw"));
            Assert.That(albumsUrls, Does.Contain("/album/in-need-of-tess"));
            Assert.That(albumsUrls, Does.Contain("/album/sound-puzzle-deluxe-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/awake"));
            Assert.That(albumsUrls, Does.Contain("/album/the-path"));
            Assert.That(albumsUrls, Does.Contain("/album/close-eyes-to-exit"));
            Assert.That(albumsUrls, Does.Contain("/album/absolute-presence"));
            Assert.That(albumsUrls, Does.Contain("/album/neida"));
            Assert.That(albumsUrls, Does.Contain("/album/presidio"));
            Assert.That(albumsUrls, Does.Contain("/album/ton"));
            Assert.That(albumsUrls, Does.Contain("/album/stay-home"));
            Assert.That(albumsUrls, Does.Contain("/album/kellion-the-stories-of-a-young-boy"));
            Assert.That(albumsUrls, Does.Contain("/album/too"));
            Assert.That(albumsUrls, Does.Contain("/album/hewn"));
            Assert.That(albumsUrls, Does.Contain("/album/falstrati"));
            Assert.That(albumsUrls, Does.Contain("/album/blue-jasmine"));
            Assert.That(albumsUrls, Does.Contain("/album/memoosh"));
            Assert.That(albumsUrls, Does.Contain("/album/push"));
            Assert.That(albumsUrls, Does.Contain("/album/slow-waves"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-inner-worlds"));
            Assert.That(albumsUrls, Does.Contain("/album/my-little-ghost"));
            Assert.That(albumsUrls, Does.Contain("/album/menoko"));
            Assert.That(albumsUrls, Does.Contain("/album/the-average-joe"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-mother-love"));
            Assert.That(albumsUrls, Does.Contain("/album/uplifting-themes-for-the-naysayer"));
            Assert.That(albumsUrls, Does.Contain("/album/two-words"));
            Assert.That(albumsUrls, Does.Contain("/album/blue-balloons-the-longest-journey"));
            Assert.That(albumsUrls, Does.Contain("/album/the-foreigner"));
            Assert.That(albumsUrls, Does.Contain("/album/unpaved"));
            Assert.That(albumsUrls, Does.Contain("/album/uprising"));
            Assert.That(albumsUrls, Does.Contain("/album/melonkoly"));
            Assert.That(albumsUrls, Does.Contain("/album/nonfiction"));
            Assert.That(albumsUrls, Does.Contain("/album/voight-kampff"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-faces-12-series-vol-5"));
            Assert.That(albumsUrls, Does.Contain("/album/clipse"));
            Assert.That(albumsUrls, Does.Contain("/album/mesektet-extnd"));
            Assert.That(albumsUrls, Does.Contain("/album/you-me-temporary"));
            Assert.That(albumsUrls, Does.Contain("/album/fuck-fight-feat-deniro-farrar"));
            Assert.That(albumsUrls, Does.Contain("/album/olvido"));
            Assert.That(albumsUrls, Does.Contain("/album/algorithms-and-ghosts"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-faces-12-series-vol-4"));
            Assert.That(albumsUrls, Does.Contain("/album/10-yrs-hhv-de-45-volume-10"));
            Assert.That(albumsUrls, Does.Contain("/album/the-branches-deluxe-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/retrospect-suite"));
            Assert.That(albumsUrls, Does.Contain("/album/the-dread-of-an-unknown-evil"));
            Assert.That(albumsUrls, Does.Contain("/album/kidsuke"));
            Assert.That(albumsUrls, Does.Contain("/album/cosmic-waves"));
            Assert.That(albumsUrls, Does.Contain("/album/tears"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-faces-12-series-vol-3"));
            Assert.That(albumsUrls, Does.Contain("/album/unfold"));
            Assert.That(albumsUrls, Does.Contain("/album/elevate-me"));
            Assert.That(albumsUrls, Does.Contain("/album/10th-anniversary-compilation"));
            Assert.That(albumsUrls, Does.Contain("/album/floating"));
            Assert.That(albumsUrls, Does.Contain("/album/luminous-things"));
            Assert.That(albumsUrls, Does.Contain("/album/find"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-faces-12-series-vol-2"));
            Assert.That(albumsUrls, Does.Contain("/album/binary-compound-vol-1"));
            Assert.That(albumsUrls, Does.Contain("/album/invisible-architect"));
            Assert.That(albumsUrls, Does.Contain("/album/hypnotic-machine"));
            Assert.That(albumsUrls, Does.Contain("/album/carving-away-the-clay"));
            Assert.That(albumsUrls, Does.Contain("/album/from-love-to-dust"));
            Assert.That(albumsUrls, Does.Contain("/album/hold-on"));
            Assert.That(albumsUrls, Does.Contain("/album/evolution-fight"));
            Assert.That(albumsUrls, Does.Contain("/album/finest-ego-faces-series-vol-1"));
            Assert.That(albumsUrls, Does.Contain("/album/the-other-side"));
            Assert.That(albumsUrls, Does.Contain("/album/organic"));
            Assert.That(albumsUrls, Does.Contain("/album/supershark"));
            Assert.That(albumsUrls, Does.Contain("/album/stroke-music"));
            Assert.That(albumsUrls, Does.Contain("/album/the-mesektet"));
            Assert.That(albumsUrls, Does.Contain("/album/its-the-trip"));
            Assert.That(albumsUrls, Does.Contain("/album/robot-robinson"));
            Assert.That(albumsUrls, Does.Contain("/album/the-branches"));
            Assert.That(albumsUrls, Does.Contain("/album/many-places"));
            Assert.That(albumsUrls, Does.Contain("/album/mini-tollbooth-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/cassiopeia"));
            Assert.That(albumsUrls, Does.Contain("/album/the-moon-comes-closer"));
            Assert.That(albumsUrls, Does.Contain("/album/songs-for-trees-and-cyborgs"));
            Assert.That(albumsUrls, Does.Contain("/album/international-summers"));
            Assert.That(albumsUrls, Does.Contain("/album/now"));
            Assert.That(albumsUrls, Does.Contain("/album/enter-the-circle"));
            Assert.That(albumsUrls, Does.Contain("/album/listen-to-them-fade-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/darker-days-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/death-star-droid-remix-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/sound-surroundings"));
            Assert.That(albumsUrls, Does.Contain("/album/death-star-droid"));
            Assert.That(albumsUrls, Does.Contain("/album/smoothness-extract-deep-night-at-ishigaki"));
            Assert.That(albumsUrls, Does.Contain("/album/falling-down"));
            Assert.That(albumsUrls, Does.Contain("/album/starship-utopia"));
            Assert.That(albumsUrls, Does.Contain("/album/silent-in-truth"));
            Assert.That(albumsUrls, Does.Contain("/album/mind-joe"));
            Assert.That(albumsUrls, Does.Contain("/album/harpy-lights-the-canopy"));
            Assert.That(albumsUrls, Does.Contain("/album/amongst-strangers"));
            Assert.That(albumsUrls, Does.Contain("/album/tribute-to-the-q4"));
            Assert.That(albumsUrls, Does.Contain("/album/audio-alchemy"));
            Assert.That(albumsUrls, Does.Contain("/album/the-lucid-effect"));
            Assert.That(albumsUrls, Does.Contain("/album/15th-anniversary-compilation"));
            Assert.That(albumsUrls, Does.Contain("/album/field-recordings"));
            Assert.That(albumsUrls, Does.Contain("/album/mesektet-10-year-anniversary-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/all-my-angles-are-right"));
            Assert.That(albumsUrls, Does.Contain("/album/belonging"));
            Assert.That(albumsUrls, Does.Contain("/album/silent-opera"));
            Assert.That(albumsUrls, Does.Contain("/album/water-for-mars"));
            Assert.That(albumsUrls, Does.Contain("/album/inner-sea"));
            Assert.That(albumsUrls, Does.Contain("/album/see-you-soon"));
            Assert.That(albumsUrls, Does.Contain("/album/darkly"));
            Assert.That(albumsUrls, Does.Contain("/album/pyramids"));
            Assert.That(albumsUrls, Does.Contain("/album/nihx"));
            Assert.That(albumsUrls, Does.Contain("/album/a-part-of-me"));
            Assert.That(albumsUrls, Does.Contain("/album/time-being-deluxe-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/are-you-anywhere"));
            Assert.That(albumsUrls, Does.Contain("/album/joy-in-the-end"));
        }
    }

    [Test]
    public void GetRelativeAlbumsUrl_Returns_Expected_Given_TympanikaudioHtml()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.TympanikaudioHtml);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetRelativeAlbumsUrlsFromArtistPage(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(99));
            Assert.That(albumsUrls, Does.Contain("/album/five-six"));
            Assert.That(albumsUrls, Does.Contain("/album/submarine"));
            Assert.That(albumsUrls, Does.Contain("/album/folgor"));
            Assert.That(albumsUrls, Does.Contain("/album/from-the-outside"));
            Assert.That(albumsUrls, Does.Contain("/album/the-faex-has-decimated"));
            Assert.That(albumsUrls, Does.Contain("/album/space-time-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/ninetynine"));
            Assert.That(albumsUrls, Does.Contain("/album/connected-worlds"));
            Assert.That(albumsUrls, Does.Contain("/album/shadows"));
            Assert.That(albumsUrls, Does.Contain("/album/emerging-organisms-vol-5"));
            Assert.That(albumsUrls, Does.Contain("/album/sercosa"));
            Assert.That(albumsUrls, Does.Contain("/album/intensive-collectivity-known-as-city"));
            Assert.That(albumsUrls, Does.Contain("/album/transience"));
            Assert.That(albumsUrls, Does.Contain("/album/noir"));
            Assert.That(albumsUrls, Does.Contain("/album/cyclicity"));
            Assert.That(albumsUrls, Does.Contain("/album/hydref"));
            Assert.That(albumsUrls, Does.Contain("/album/innerheaven"));
            Assert.That(albumsUrls, Does.Contain("/album/the-gift-of-affliction"));
            Assert.That(albumsUrls, Does.Contain("/album/portents-call"));
            Assert.That(albumsUrls, Does.Contain("/album/hollow-worlds"));
            Assert.That(albumsUrls, Does.Contain("/album/winter-eyes"));
            Assert.That(albumsUrls, Does.Contain("/album/fear-of-the-unknown"));
            Assert.That(albumsUrls, Does.Contain("/album/digital-exorcist"));
            Assert.That(albumsUrls, Does.Contain("/album/room-78"));
            Assert.That(albumsUrls, Does.Contain("/album/indistinct-face"));
            Assert.That(albumsUrls, Does.Contain("/album/mgnovenie"));
            Assert.That(albumsUrls, Does.Contain("/album/lights"));
            Assert.That(albumsUrls, Does.Contain("/album/accretion"));
            Assert.That(albumsUrls, Does.Contain("/album/an-g-mi"));
            Assert.That(albumsUrls, Does.Contain("/album/at-the-end-of-it-all-remixed"));
            Assert.That(albumsUrls, Does.Contain("/album/elf-morgen"));
            Assert.That(albumsUrls, Does.Contain("/album/raintree"));
            Assert.That(albumsUrls, Does.Contain("/album/embark-on-departure"));
            Assert.That(albumsUrls, Does.Contain("/album/at-the-end-of-it-all"));
            Assert.That(albumsUrls, Does.Contain("/album/hollow"));
            Assert.That(albumsUrls, Does.Contain("/album/new-world-march-special-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/the-past-is-my-shadow"));
            Assert.That(albumsUrls, Does.Contain("/album/overgrown"));
            Assert.That(albumsUrls, Does.Contain("/album/geosynchron"));
            Assert.That(albumsUrls, Does.Contain("/album/deleted-1999-2006"));
            Assert.That(albumsUrls, Does.Contain("/album/horizon"));
            Assert.That(albumsUrls, Does.Contain("/album/inward-structures"));
            Assert.That(albumsUrls, Does.Contain("/album/desert"));
            Assert.That(albumsUrls, Does.Contain("/album/emerging-organisms-vol-4"));
            Assert.That(albumsUrls, Does.Contain("/album/dead-market"));
            Assert.That(albumsUrls, Does.Contain("/album/second-life"));
            Assert.That(albumsUrls, Does.Contain("/album/queue"));
            Assert.That(albumsUrls, Does.Contain("/album/night-gallery"));
            Assert.That(albumsUrls, Does.Contain("/album/orbitus"));
            Assert.That(albumsUrls, Does.Contain("/album/scintilla"));
            Assert.That(albumsUrls, Does.Contain("/album/natures-twin-tendencies"));
            Assert.That(albumsUrls, Does.Contain("/album/etched-in-salt"));
            Assert.That(albumsUrls, Does.Contain("/album/all-standing-room-in-the-goodnight-saloon"));
            Assert.That(albumsUrls, Does.Contain("/album/conclusion"));
            Assert.That(albumsUrls, Does.Contain("/album/10-10pm"));
            Assert.That(albumsUrls, Does.Contain("/album/autumn-fields"));
            Assert.That(albumsUrls, Does.Contain("/album/void"));
            Assert.That(albumsUrls, Does.Contain("/album/i-will-wait"));
            Assert.That(albumsUrls, Does.Contain("/album/on-the-first-of-november"));
            Assert.That(albumsUrls, Does.Contain("/album/symbiont-underground"));
            Assert.That(albumsUrls, Does.Contain("/album/turbulences"));
            Assert.That(albumsUrls, Does.Contain("/album/64-light-years-away"));
            Assert.That(albumsUrls, Does.Contain("/album/l36"));
            Assert.That(albumsUrls, Does.Contain("/album/blood"));
            Assert.That(albumsUrls, Does.Contain("/album/the-muse-in-the-machine"));
            Assert.That(albumsUrls, Does.Contain("/album/emerging-organisms-vol-3"));
            Assert.That(albumsUrls, Does.Contain("/album/nothing-lasts"));
            Assert.That(albumsUrls, Does.Contain("/album/return-to-childhood"));
            Assert.That(albumsUrls, Does.Contain("/album/x-was-never-like-this"));
            Assert.That(albumsUrls, Does.Contain("/album/oppidan"));
            Assert.That(albumsUrls, Does.Contain("/album/plastic"));
            Assert.That(albumsUrls, Does.Contain("/album/evolution"));
            Assert.That(albumsUrls, Does.Contain("/album/full-spectrum-dominance"));
            Assert.That(albumsUrls, Does.Contain("/album/where-once-were-exit-wounds"));
            Assert.That(albumsUrls, Does.Contain("/album/black-brothel"));
            Assert.That(albumsUrls, Does.Contain("/album/a-bright-cut-across-velvet-sky"));
            Assert.That(albumsUrls, Does.Contain("/album/fallen-clouds"));
            Assert.That(albumsUrls, Does.Contain("/album/the-things-that-disappear-when-i-close-my-eyes"));
            Assert.That(albumsUrls, Does.Contain("/album/epiphora"));
            Assert.That(albumsUrls, Does.Contain("/album/sonnambula"));
            Assert.That(albumsUrls, Does.Contain("/album/emerging-organisms-vol-2"));
            Assert.That(albumsUrls, Does.Contain("/album/severed"));
            Assert.That(albumsUrls, Does.Contain("/album/no"));
            Assert.That(albumsUrls, Does.Contain("/album/surge"));
            Assert.That(albumsUrls, Does.Contain("/album/forgotten-on-the-other-side-of-the-tracks"));
            Assert.That(albumsUrls, Does.Contain("/album/the-institute-of-random-events"));
            Assert.That(albumsUrls, Does.Contain("/album/rise"));
            Assert.That(albumsUrls, Does.Contain("/album/love-no-longer-lives-here"));
            Assert.That(albumsUrls, Does.Contain("/album/carbon"));
            Assert.That(albumsUrls, Does.Contain("/album/gravedigger"));
            Assert.That(albumsUrls, Does.Contain("/album/the-witching-hour"));
            Assert.That(albumsUrls, Does.Contain("/album/circumsounds"));
            Assert.That(albumsUrls, Does.Contain("/album/bone-music"));
            Assert.That(albumsUrls, Does.Contain("/album/cloned-other-side-of-evolution"));
            Assert.That(albumsUrls, Does.Contain("/album/approach"));
            Assert.That(albumsUrls, Does.Contain("/album/elekatota-the-other-side-of-the-tracks"));
            Assert.That(albumsUrls, Does.Contain("/album/the-rakes-progress"));
            Assert.That(albumsUrls, Does.Contain("/album/emerging-organisms"));
            Assert.That(albumsUrls, Does.Contain("/album/tympanik-audio-exlusive-dj-mix"));
        }
    }
}
