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
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Goataholicskjald()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Goataholicskjald_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(1));
            Assert.That(albumsUrls, Does.Contain("/album/dogma"));
        }
    }

    [Test]
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Cratediggers()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Cratediggers_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(1));
            Assert.That(albumsUrls, Does.Contain("/album/concrete-canvases"));
        }
    }

    [Test]
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Mstrvlk()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Mstrvlk_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(3));
            Assert.That(albumsUrls, Does.Contain("/album/dec-cem-ber"));
            Assert.That(albumsUrls, Does.Contain("/album/mv-1"));
            Assert.That(albumsUrls, Does.Contain("/album/-"));
        }
    }

    [Test]
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Weneverlearnedtolive()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Weneverlearnedtolive_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(6));
            Assert.That(albumsUrls, Does.Contain("/album/ode"));
            Assert.That(albumsUrls, Does.Contain("/album/the-sleepwalk-transmissions"));
            Assert.That(albumsUrls, Does.Contain("/album/silently-i-threw-them-skyward"));
            Assert.That(albumsUrls, Does.Contain("/track/crystalline-so-serene"));
            Assert.That(albumsUrls, Does.Contain("/track/shadows-in-hibernation"));
            Assert.That(albumsUrls, Does.Contain("/album/s-t"));
        }
    }

    [Test]
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Afterdarkrecordings()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Afterdarkrecordings_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(22));
            Assert.That(albumsUrls, Does.Contain("/album/misc-trax"));
            Assert.That(albumsUrls, Does.Contain("/album/time-travel-records"));
            Assert.That(albumsUrls, Does.Contain("/album/paradise-records"));
            Assert.That(albumsUrls, Does.Contain("/track/empire"));
            Assert.That(albumsUrls, Does.Contain("/album/adr"));
            Assert.That(albumsUrls, Does.Contain("/album/afterdark-collection"));
            Assert.That(albumsUrls, Does.Contain("/album/set-your-body-free-amp-destructor"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-5"));
            Assert.That(albumsUrls, Does.Contain("/album/the-guinness-track"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-1"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-2-2"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-3-2"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-4-2"));
            Assert.That(albumsUrls, Does.Contain("/album/the-elevator-odyssey"));
            Assert.That(albumsUrls, Does.Contain("/track/guinness-track-peshay-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/guinness-track-remastered"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-2"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-3"));
            Assert.That(albumsUrls, Does.Contain("/album/energizer-4"));
            Assert.That(albumsUrls, Does.Contain("/album/the-energizers-1"));
            Assert.That(albumsUrls, Does.Contain("/album/adr-unreleased-tracks"));
            Assert.That(albumsUrls, Does.Contain("/album/universe-records"));
        }
    }

    [Test]
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Affektrecordings()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Affektrecordings_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

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
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Projectmooncircle()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Projectmooncircle_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

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
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Tympanikaudio()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Tympanikaudio_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

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

    [Test]
    public void GetReferredAlbumsRelativeUrls_Returns_Expected_Given_Astropilotmusic()
    {
        // Arrange
        var htmlFile = ResourceAccessor.GetFileInfo(ResourceId.Music_Astropilotmusic_Html);
        var htmlContent = File.ReadAllText(htmlFile.FullName);

        // Act
        var albumsUrls = _sut.GetReferredAlbumsRelativeUrls(htmlContent);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(albumsUrls, Has.Count.EqualTo(372));
            Assert.That(albumsUrls, Does.Contain("/album/fruits-of-the-imagination-20-years-of-imagination"));
            Assert.That(albumsUrls, Does.Contain("/album/the-vault-of-a-thousand-galaxies"));
            Assert.That(albumsUrls, Does.Contain("/album/awakening-chimes"));
            Assert.That(albumsUrls, Does.Contain("/album/liquid-silence"));
            Assert.That(albumsUrls, Does.Contain("/album/snowfall"));
            Assert.That(albumsUrls, Does.Contain("/album/above-live-in-krasnoyarsk-2011"));
            Assert.That(albumsUrls, Does.Contain("/album/iriy-remaster-2025"));
            Assert.That(albumsUrls, Does.Contain("/album/a-memory-dissolving"));
            Assert.That(albumsUrls, Does.Contain("/album/mental-detachment"));
            Assert.That(albumsUrls, Does.Contain("/album/this-quiet-was-once-alive"));
            Assert.That(albumsUrls, Does.Contain("/album/rain-affairs"));
            Assert.That(albumsUrls, Does.Contain("/album/fading-sun-endless-blue"));
            Assert.That(albumsUrls, Does.Contain("/album/atmospheric"));
            Assert.That(albumsUrls, Does.Contain("/album/light-through-branches"));
            Assert.That(albumsUrls, Does.Contain("/album/cosmic-dance"));
            Assert.That(albumsUrls, Does.Contain("/album/paranormal-passivity"));
            Assert.That(albumsUrls, Does.Contain("/album/structure"));
            Assert.That(albumsUrls, Does.Contain("/album/toolkit-serum-pack"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-i-iii-15-years-anniversary-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/the-ashes-of-collapsed-dreams"));
            Assert.That(albumsUrls, Does.Contain("/album/luminosity"));
            Assert.That(albumsUrls, Does.Contain("/album/lifelines"));
            Assert.That(albumsUrls, Does.Contain("/album/ornament-chillout-mix-2023"));
            Assert.That(albumsUrls, Does.Contain("/album/rocky-river"));
            Assert.That(albumsUrls, Does.Contain("/album/the-dawn-beyond-the-edge"));
            Assert.That(albumsUrls, Does.Contain("/album/the-time-machine"));
            Assert.That(albumsUrls, Does.Contain("/album/lost-in-silence"));
            Assert.That(albumsUrls, Does.Contain("/album/ambient-2024-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/space-affair"));
            Assert.That(albumsUrls, Does.Contain("/album/yearbook-2024-2"));
            Assert.That(albumsUrls, Does.Contain("/album/yearbook-2024-free-mixed-version"));
            Assert.That(albumsUrls, Does.Contain("/album/stellar-textures"));
            Assert.That(albumsUrls, Does.Contain("/album/retrospect"));
            Assert.That(albumsUrls, Does.Contain("/album/rebirth"));
            Assert.That(albumsUrls, Does.Contain("/album/slow-down"));
            Assert.That(albumsUrls, Does.Contain("/album/stellar-nomads"));
            Assert.That(albumsUrls, Does.Contain("/album/flame"));
            Assert.That(albumsUrls, Does.Contain("/album/the-moon-is-a-harsh-mistress"));
            Assert.That(albumsUrls, Does.Contain("/album/summers-fading-echo-24-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/thirty-three-2024-remaster-24-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/echoes-from-the-past-remixes"));
            Assert.That(albumsUrls, Does.Contain("/album/winds-of-solaris"));
            Assert.That(albumsUrls, Does.Contain("/album/in-dub"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-iv-youniverse-2024-remaster"));
            Assert.That(albumsUrls, Does.Contain("/album/infusion"));
            Assert.That(albumsUrls, Does.Contain("/album/the-chance-2024-edit"));
            Assert.That(albumsUrls, Does.Contain("/album/in-the-arms-of-time"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-change-sleep-edit"));
            Assert.That(albumsUrls, Does.Contain("/album/kosmonaut"));
            Assert.That(albumsUrls, Does.Contain("/album/between-the-mountains-and-the-sky-24-bits-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/skyscraper-start-up"));
            Assert.That(albumsUrls, Does.Contain("/album/project-12013-vol-2-driver"));
            Assert.That(albumsUrls, Does.Contain("/album/sunrise-at-the-summit-extended-version-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/soul-dreamer-2024-edit"));
            Assert.That(albumsUrls, Does.Contain("/album/true-tone"));
            Assert.That(albumsUrls, Does.Contain("/album/the-best-of-ucp-mixed-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/the-best-of-ucp"));
            Assert.That(albumsUrls, Does.Contain("/album/to-see-open-earth"));
            Assert.That(albumsUrls, Does.Contain("/album/bipolar-slowed-and-reverbed"));
            Assert.That(albumsUrls, Does.Contain("/album/time-and-space-solar-walk-star-walk-soundtracks-collection"));
            Assert.That(albumsUrls, Does.Contain("/album/lonely-planet"));
            Assert.That(albumsUrls, Does.Contain("/album/the-colours-of-horizon-slowed-and-reverbed"));
            Assert.That(albumsUrls, Does.Contain("/album/relax-man-24bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/dubstation-mixes"));
            Assert.That(albumsUrls, Does.Contain("/album/timeless-sky"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-ii-ice-remastered-mixed"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-ii-ice-remastered"));
            Assert.That(albumsUrls, Does.Contain("/album/dusty-memories"));
            Assert.That(albumsUrls, Does.Contain("/album/away-from-the-bustle"));
            Assert.That(albumsUrls, Does.Contain("/album/blast-from-the-plast"));
            Assert.That(albumsUrls, Does.Contain("/album/intention-ucp-edit"));
            Assert.That(albumsUrls, Does.Contain("/album/project-12013-vol-1-home"));
            Assert.That(albumsUrls, Does.Contain("/album/rhododenron-drone-edit-24-bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/redsilence-remixes"));
            Assert.That(albumsUrls, Does.Contain("/album/magic-moment"));
            Assert.That(albumsUrls, Does.Contain("/album/astrophere-remastered-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/interstellar-lullabies-an-10-hour-cosmic-sojourn"));
            Assert.That(albumsUrls, Does.Contain("/album/out-of-space-ucp-edit"));
            Assert.That(albumsUrls, Does.Contain("/album/rewind-selected-chillgressive-downtempo-works-2012-2016"));
            Assert.That(albumsUrls, Does.Contain("/album/intention"));
            Assert.That(albumsUrls, Does.Contain("/album/titles-astropilot-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/styleus-24bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/svarga-long-form-drone-version-24-bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/alpha-seeds-remastered-free-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/alpha-seeds-remastered-2"));
            Assert.That(albumsUrls, Does.Contain("/album/starfall"));
            Assert.That(albumsUrls, Does.Contain("/album/right-in-the-night-tenet-audio-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/timewheel"));
            Assert.That(albumsUrls, Does.Contain("/album/timewheel-free-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/noise-from-the-wire"));
            Assert.That(albumsUrls, Does.Contain("/album/going-downhill"));
            Assert.That(albumsUrls, Does.Contain("/album/heritage-tales-version-2024"));
            Assert.That(albumsUrls, Does.Contain("/album/stellar"));
            Assert.That(albumsUrls, Does.Contain("/album/rea-24bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/out-of-space-24bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/event-horizon-long-form-drone-version-24-bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/apollo-8-code-slowed-and-reverbed"));
            Assert.That(albumsUrls, Does.Contain("/album/labyrinths-of-the-galactic-soul"));
            Assert.That(albumsUrls, Does.Contain("/album/mistress-of-the-moon"));
            Assert.That(albumsUrls, Does.Contain("/album/yearbook-2023-ambient-edition"));
            Assert.That(albumsUrls, Does.Contain("/album/sea-baby"));
            Assert.That(albumsUrls, Does.Contain("/album/yearbook-2023"));
            Assert.That(albumsUrls, Does.Contain("/album/alien-galaxy"));
            Assert.That(albumsUrls, Does.Contain("/album/l-i-m-i-t-l-e-s-s"));
            Assert.That(albumsUrls, Does.Contain("/album/in-search-of-atlantis-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/singularity"));
            Assert.That(albumsUrls, Does.Contain("/album/elysium-revisited-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/valley-of-harmony-piano-versions-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/cloudfields-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/dream-chaser"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-remastered-2023"));
            Assert.That(albumsUrls, Does.Contain("/album/redsilence"));
            Assert.That(albumsUrls, Does.Contain("/album/eternity-a-e-r-o-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/arrakis"));
            Assert.That(albumsUrls, Does.Contain("/album/abyssal-adventurers-logs-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/sanctum"));
            Assert.That(albumsUrls, Does.Contain("/album/dubby"));
            Assert.That(albumsUrls, Does.Contain("/album/frames-of-silence-revisited"));
            Assert.That(albumsUrls, Does.Contain("/album/the-toad-shaman-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/astral-asteroid-mobitex-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/gleaming-on-the-horizon-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-elusive-ii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-elusive-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/millions-light-years-away-long-form-drone-version-24-bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/frozen-moment-a-e-r-o-remix-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/path-to-the-spheres-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/echoes-of-tomorrow-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/orchid-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/when-time-stands-still-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/ultraboundary-flight-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/valley-of-harmony-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/astral-asteroid-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/spacetime-revisited-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/whiff-of-eternity-long-form-drone-version-24-bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/close-to-the-stars-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/children-tenet-audio-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/spiral-of-galaxies-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/distant-worlds-long-form-drone-version-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/eternity-unusual-cosmic-process-ambient-rework-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/interstellar-journeys-ethereal-soundscapes-from-above-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/sweet-sadness-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/re-ambient-reworks-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/weightless-mind-24bit-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/tenderness-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/atlantis-ambient-rework-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/missing-pieces-remastered-2023-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/between-continents-revisited-and-remastered-2023-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-archive-iv-ambient-worx-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-change-dmitriys-piano-rework-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/global-trance-grooves-mix-2019"));
            Assert.That(albumsUrls, Does.Contain("/album/memory-lane-unusual-cosmic-process-ambient-rework-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/paraselene-revisited-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/spectrum-progress-remix-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/hidden-ecosphere-ucp-ambient-rework-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/utopia-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/spectrum-progress"));
            Assert.That(albumsUrls, Does.Contain("/album/optical-illusion-unusual-cosmic-process-ambient-rework"));
            Assert.That(albumsUrls, Does.Contain("/album/2022"));
            Assert.That(albumsUrls, Does.Contain("/album/kaleidoscope-2022-year-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/soul-dreamers"));
            Assert.That(albumsUrls, Does.Contain("/album/hopes-and-dreams-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/marth-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/ad-astra-ambient-reworks-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/memory-lane"));
            Assert.That(albumsUrls, Does.Contain("/album/frozen-moment-ambient-rework"));
            Assert.That(albumsUrls, Does.Contain("/album/quazary-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/far-behind-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/intrigue-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/haze"));
            Assert.That(albumsUrls, Does.Contain("/album/fractal-forms-u-c-p-in-dub-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/peaceful-music-for-challenging-times-ambient-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/cassiopeia-ambient-rework"));
            Assert.That(albumsUrls, Does.Contain("/album/atlantis"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-changes-2022-edit"));
            Assert.That(albumsUrls, Does.Contain("/album/mission-poseidon-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/ad-astra-ambient-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/blind-observer-mix-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/listening-to-nature-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/after-day"));
            Assert.That(albumsUrls, Does.Contain("/album/mria"));
            Assert.That(albumsUrls, Does.Contain("/album/falling-snowflakes"));
            Assert.That(albumsUrls, Does.Contain("/album/echoes"));
            Assert.That(albumsUrls, Does.Contain("/album/pineland"));
            Assert.That(albumsUrls, Does.Contain("/album/serenity"));
            Assert.That(albumsUrls, Does.Contain("/album/millions-light-years-away-revisited"));
            Assert.That(albumsUrls, Does.Contain("/album/dynamics"));
            Assert.That(albumsUrls, Does.Contain("/album/2021"));
            Assert.That(albumsUrls, Does.Contain("/album/depths-of-consciousness-mix-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/the-nauts-log-chapter-1-4"));
            Assert.That(albumsUrls, Does.Contain("/album/earthwalk-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/blurry-night-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/bipolar"));
            Assert.That(albumsUrls, Does.Contain("/album/synesthesia"));
            Assert.That(albumsUrls, Does.Contain("/album/unusual-sentinel"));
            Assert.That(albumsUrls, Does.Contain("/album/fractal-forms"));
            Assert.That(albumsUrls, Does.Contain("/album/breathe-the-light"));
            Assert.That(albumsUrls, Does.Contain("/album/the-kite-breaks-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-change-tenet-audio-remix-24bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-cycle-of-being-ii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/frozen-moment-breaks-remix-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-new-trigger-pt-1-tenet-audio-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/radar-ii"));
            Assert.That(albumsUrls, Does.Contain("/album/stardust-adventure-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-kite-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-phantom-ujo-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/first-orbit-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/i-believe-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-phantom-pt-2-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/midgard-earth-remastered-2021-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/ad-astra-remixes-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/discoveries-remastered-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-change-pt-2-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/perceptions-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/rose-peak-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/re-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/takeoff-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/relaxed-dancing"));
            Assert.That(albumsUrls, Does.Contain("/album/in-the-silence-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/radar-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/diary-of-a-restless-mind-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-change-ujo-remix-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/last-dance-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/2020"));
            Assert.That(albumsUrls, Does.Contain("/album/laura-free-download-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/wind-of-change-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/elements-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/sanctum-psybient-edit-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/past-fragments-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/cassiopeia-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/sleeping-pinewood-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/inner-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/dark-skies-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/dharma-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/desolation-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/random-thoughts-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/biogenesis-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/soul-dreamer-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/new-merch-details-all-info-inside"));
            Assert.That(albumsUrls, Does.Contain("/album/voice-of-earth-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-chill-stompers-ii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/existence-of-life-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-chill-stompers-i-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/at-the-bottom-of-the-world-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/inside-human-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/lost-radiation"));
            Assert.That(albumsUrls, Does.Contain("/album/arrival-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/everything-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-mist-remastered-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/reveries-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/a-world-for-you-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/re-mixed-vol-ii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/re-mixed-vol-i-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/adult-fairy-tale-an-ambient-dj-mix-2010-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/fase-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-archive-iii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/innerpath-an-ambient-dj-mix-2009-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/the-archive-ii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/sinestesia"));
            Assert.That(albumsUrls, Does.Contain("/album/macronaut-free-download-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/faded-walls-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/the-archive-i-shamanium-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/time-of-memories"));
            Assert.That(albumsUrls, Does.Contain("/album/phantom-chains-an-ambient-dj-mix-2010-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/2019-vol-ii-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/2019-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/mysterious-landing-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/a-dive-to-the-deepness-dj-mix-2010-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/unspeakable-dj-mix-2009"));
            Assert.That(albumsUrls, Does.Contain("/album/in-the-process-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/live-at-atmasfera360"));
            Assert.That(albumsUrls, Does.Contain("/album/back-to-life"));
            Assert.That(albumsUrls, Does.Contain("/track/chill-out-planet-radioshow-on-megapolis-895-fm-08-11-2019"));
            Assert.That(albumsUrls, Does.Contain("/album/life-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/seeds-of-life-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/organic-remixes-updated-remastered-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/liquid-dimension"));
            Assert.That(albumsUrls, Does.Contain("/album/love-sine-astropilot-remix"));
            Assert.That(albumsUrls, Does.Contain("/album/transform"));
            Assert.That(albumsUrls, Does.Contain("/album/iriy-remastered-2019-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/crystal-poems-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/sand-in-the-wind-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/soul-surfers-remastered-2019-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/star-walk-remastered-2019-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/mosaic-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/revival-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/sound-signal-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-iii-unseen-chapters"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-iii-event-horizon-remastered-2019-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/dandelion"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-ii-remastered-2019-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-remastered-2019-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/dreams-about-meditative-soundscapes"));
            Assert.That(albumsUrls, Does.Contain("/album/artifacts-vol-2-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/best-of-2018-24-bits-free-download"));
            Assert.That(albumsUrls, Does.Contain("/album/artifacts-vol-1-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/heritage-tales-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/cloudland-soundtrack-works"));
            Assert.That(albumsUrls, Does.Contain("/album/raneous"));
            Assert.That(albumsUrls, Does.Contain("/album/the-bhaktas-remixes"));
            Assert.That(albumsUrls, Does.Contain("/album/a-s-t-r-o-s-p-h-e-r-e-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/otherside-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/aqua-perception-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/closing-set-at-samsara-festival-2018"));
            Assert.That(albumsUrls, Does.Contain("/album/dragonfly"));
            Assert.That(albumsUrls, Does.Contain("/album/heritage-chapter-iii-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/enta-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/heritage-chapter-ii"));
            Assert.That(albumsUrls, Does.Contain("/album/samplepack"));
            Assert.That(albumsUrls, Does.Contain("/album/ellipse"));
            Assert.That(albumsUrls, Does.Contain("/album/frames-of-silence-16-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/frames-of-silence-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/heritage-chapter-i"));
            Assert.That(albumsUrls, Does.Contain("/album/samsara-festival-2016-closing-set"));
            Assert.That(albumsUrls, Does.Contain("/album/introspection"));
            Assert.That(albumsUrls, Does.Contain("/album/this-fire"));
            Assert.That(albumsUrls, Does.Contain("/album/heritage-foreword"));
            Assert.That(albumsUrls, Does.Contain("/album/live-at-boom-2012"));
            Assert.That(albumsUrls, Does.Contain("/album/live-at-aurora-festival-2011"));
            Assert.That(albumsUrls, Does.Contain("/album/best-of-2017"));
            Assert.That(albumsUrls, Does.Contain("/album/random-perspective-24-bits-remaster"));
            Assert.That(albumsUrls, Does.Contain("/album/space-ambient-live-set-for-winter-solstice-2012-psychill-di-fm"));
            Assert.That(albumsUrls, Does.Contain("/track/hikuri"));
            Assert.That(albumsUrls, Does.Contain("/album/best-of-2017-free-mix"));
            Assert.That(albumsUrls, Does.Contain("/album/interstellar-horizon-16-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/interstellar-horizon-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/track/promo-live-set-for-tree-of-life-2013"));
            Assert.That(albumsUrls, Does.Contain("/album/near-the-horizon-free-release"));
            Assert.That(albumsUrls, Does.Contain("/album/out-of-control-24-bits-remaster"));
            Assert.That(albumsUrls, Does.Contain("/album/the-journey-24-bits-remaster"));
            Assert.That(albumsUrls, Does.Contain("/album/alpha-seeds-16-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/alpha-seeds-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/thirty-three-16-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/thirty-three-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/spacetime-16-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/spacetime-24-bit"));
            Assert.That(albumsUrls, Does.Contain("/album/lost-space-device-remastered-2017"));
            Assert.That(albumsUrls, Does.Contain("/album/elysium"));
            Assert.That(albumsUrls, Does.Contain("/album/solar-walk-iv-youniverse-24-bits-studio-master"));
            Assert.That(albumsUrls, Does.Contain("/album/the-waste-lands-24-bits-studio-master"));
            Assert.That(albumsUrls, Does.Contain("/album/weightlessness-remastered-2017"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-ii-fire-amva3"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-ii-ice-amva02"));
            Assert.That(albumsUrls, Does.Contain("/album/natures-everlasting-glory-24bits"));
            Assert.That(albumsUrls, Does.Contain("/album/fruits-of-the-imagination-remastered-2016"));
            Assert.That(albumsUrls, Does.Contain("/album/all-my-days"));
            Assert.That(albumsUrls, Does.Contain("/album/time-architect-24-bits-studio-master"));
            Assert.That(albumsUrls, Does.Contain("/album/between-continents-16-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/between-continents-24-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/flashbacks-24-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-amva01"));
            Assert.That(albumsUrls, Does.Contain("/album/colours-amva01-16-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/relicts-24-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/escape-24-bits"));
            Assert.That(albumsUrls, Does.Contain("/album/ray-of-light-ep"));
            Assert.That(albumsUrls, Does.Contain("/album/unexpected-past-from-the-archive-2002-2011"));
            Assert.That(albumsUrls, Does.Contain("/album/collection-of-old-tracks-2005-2010"));
            Assert.That(albumsUrls, Does.Contain("/album/emptiness"));
            Assert.That(albumsUrls, Does.Contain("/album/ghost-mixes"));
            Assert.That(albumsUrls, Does.Contain("/album/where-branches-lean-to-air"));
            Assert.That(albumsUrls, Does.Contain("/album/solace"));
            Assert.That(albumsUrls, Does.Contain("/album/maps-for-nowhere"));
            Assert.That(albumsUrls, Does.Contain("/album/veins-of-the-earth"));
            Assert.That(albumsUrls, Does.Contain("/album/the-topology-of-dreams"));
            Assert.That(albumsUrls, Does.Contain("/album/the-geometry-of-quiet"));
            Assert.That(albumsUrls, Does.Contain("/album/mist-keeps-the-path"));
            Assert.That(albumsUrls, Does.Contain("/album/enlightenment"));
            Assert.That(albumsUrls, Does.Contain("/album/cloud-diagram"));
            Assert.That(albumsUrls, Does.Contain("/album/promoset-2011-for-psyradio-dj-set-at-private-party-kyiv-2011"));
            Assert.That(albumsUrls, Does.Contain("/album/almost-in-silence"));
            Assert.That(albumsUrls, Does.Contain("/album/imagination"));
            Assert.That(albumsUrls, Does.Contain("/album/dreaming-of-eden"));
            Assert.That(albumsUrls, Does.Contain("/album/paradigm"));
        }
    }
}
