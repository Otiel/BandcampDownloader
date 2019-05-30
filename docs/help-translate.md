# Help translate

## Option 1: the easy way

[![Translation status](https://hosted.weblate.org/widgets/bandcampdownloader/-/287x66-white.png)](https://hosted.weblate.org/engage/bandcampdownloader)

If you wish to help translate this application, head over to the [Weblate project](https://hosted.weblate.org/engage/bandcampdownloader). After creating an account on Weblate, you will be able to:
* Modify the existing translations,
* Make suggestions to the existing translations,
* Add a new language.

## Option 2: editing the translation files

If you prefer directly editing the strings files, the following should get you started:

### Edit the strings files

Translations are stored in resources (_resx_) files, one file per language. For instance, the following translations exist:
* English: [_Resources.resx_](/src/BandcampDownloader/Properties/Resources.resx)
* French: [_Resources.fr.resx_](/src/BandcampDownloader/Properties/Resources.fr.resx)
* German: [_Resources.de.resx_](/src/BandcampDownloader/Properties/Resources.de.resx)
* ...

In order to modify the translations, you can either edit the files with your favorite text editor, or use a tool such as [Zeta Resource Editor](https://www.zeta-resource-editor.com) or [ResX Resource Manager](https://github.com/tom-englert/ResXResourceManager). When editing a _resx_ file with a text editor, you will find the translations at the end of the file. A translation looks like this:
```
<data name="buttonStart" xml:space="preserve">
  <value>Start _download</value>
</data>
```

Only the `<value>...</value>` node should be modified. The `name=` attribute should give you some context in addition to the English translation. Missing translations will not appear in the language file, you will have to run a diff between your language file and the source file ([_Resources.resx_](/src/BandcampDownloader/Properties/Resources.resx)) and copy the whole `<data>...</data>` node for the missing translation.

Make sure you keep an access key (also called mnemonic) when there is one: the underscore character `_` should precede the letter used as the access key. Ideally, there shouldn't be any duplicate mnemonics on the same window.

### Create a new language

If you wish to create a new language file, simply duplicate the English file (_Resources.resx_) in the same place and rename it using the correct [culture code](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo?view=netframework-4.7.2#culture-names-and-identifiers) (for instance: _Resources.fr.resx_, _Resources.fr-CA.resx_...):

>The name is a combination of an [ISO 639 two-letter](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) lowercase culture code associated with a language and an ISO 3166 two-letter uppercase subculture code associated with a country or region.

### Submit changes

Once you're done, commit your changes and create a pull request.

## Closing

If you're not sure of the translation by lack of context for instance, don't hesitate to open a [new issue](https://github.com/Otiel/BandcampDownloader/issues/new) and ask. Any effort will be appreciated, even if you cannot provide a complete translation. **Thanks in advance for your help!**