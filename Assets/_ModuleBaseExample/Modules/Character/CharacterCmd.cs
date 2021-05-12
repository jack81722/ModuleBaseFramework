using Fungus;
using ModuleBased.FungusPlugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example {
    [CommandInfo("Modules", "CharacterMod", "")]
    public class CharacterCmd : GenericCmd<ICharacterModule> { }
}