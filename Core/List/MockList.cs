using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.List
{
    public class MockList
    {
        public UnitsServerManager unitManagerCore { get; set; }
        Vector2 startPosRatis = new Vector2(-1f, 0);
        
        Vector2 startPosRatis2 = new Vector2(2.75f, -0.6f);


        Vector2 startPosAbomination = new Vector2(5, 0);
        Vector2 startPosGoblins = new Vector2(6f, 0.14f);
        Vector2 startPosDwarfos = new Vector2(0.5f, 1.15f);
        Vector2 startPosSlayers = new Vector2(2.8f, 0.65f);
        Vector2 startPosGyros = new Vector2(4.5f, 1.35f);
        Vector2 startPosElders = new Vector2(5.8f, 1.25f);

        public MockList(int numberList)
        {
            unitManagerCore = new UnitsServerManager();
            switch (numberList)
            {
                case 1:
                    MockList1();
                    break;
                default:
                    MockList1();
                    break;
            }
        }
        private void MockList1()
        {

            //EXAMPLE
            //BaseUnit baseunit = new BaseUnit(unitType.Name, widthRank, Formation_type.CLOSE_ORDER, new List<string> { "Reglaespecial1", "Reglaespecial2" }, troops);

            // ORCSSSSSSSSS
            BaseUnit orcs = unitManagerCore.CreateNewUnit("Orcs", new List<Character>(), 5, 20, startPosRatis2, 20);
            unitManagerCore.addPlayerUnit(orcs);

            Character goblinWizard = CodexAll.Instance.getCharCodex("Goblin Wizard");
            List<Character> listChar = new List<Character>();
            listChar.Add(goblinWizard);
            BaseUnit goblins = unitManagerCore.CreateNewUnit("Goblins", listChar, 5, 17, startPosGoblins, 15);
            unitManagerCore.addPlayerUnit(goblins);

            BaseUnit dragon = unitManagerCore.CreateNewUnit("Abomination", new List<Character>(), 1, 1, startPosAbomination, 15);
            unitManagerCore.addPlayerUnit(dragon);

            //// PLAYER2
            ///DWARSSSSSSSSSSS
            BaseUnit dwarfos = unitManagerCore.CreateNewUnit("Dwarf Warriors", new List<Character>(), 5, 13, startPosDwarfos, 130);
            unitManagerCore.addEnemyUnit(dwarfos);

            BaseUnit slayers = unitManagerCore.CreateNewUnit("Slayers", new List<Character>(), 5, 7, startPosSlayers, 200);
            unitManagerCore.addEnemyUnit(slayers);

            BaseUnit gyros = unitManagerCore.CreateNewUnit("Gyrocopters", new List<Character>(), 1, 1, startPosGyros, 190);
            unitManagerCore.addEnemyUnit(gyros);


            BaseUnit elders = unitManagerCore.CreateNewUnit("Elder Dwarfs", new List<Character>(), 5, 12, startPosElders, 170);
            unitManagerCore.addEnemyUnit(elders);
        }
    }
}
