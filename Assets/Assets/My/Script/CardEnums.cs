
using UnityEngine;

namespace CardEnums{

    public enum HAND_VALUE
    {
        NOTHING,

        BURST_PLAYER,
        BURST_DEALER,

        //VALUE3 = 3,
        VALUE4 = 4,
        VALUE5 = 5,
        VALUE6 = 6,
        VALUE7 = 7,
        VALUE8 = 8,
        VALUE9 = 9,
        VALUE10 = 10,
        VALUE11 = 11,
        VALUE12 = 12,
        VALUE13 = 13,
        VALUE14 = 14,
        VALUE15 = 15,
        VALUE16 = 16,
        VALUE17 = 17,
        VALUE18 = 18,
        VALUE19 = 19,
        VALUE20 = 20,
        VALUE21 = 21,

        BLACKJACK = 22,
    }

    public enum ChoiceKind
    {
        NotDetermined,

        Hit = 1,
        Stand = 2,
        DoubleDown = 4,
        Split = 8,
        Surrender = 16,

        NotInsurance,
        Insurance
    }

    public enum SITUATION_KIND
    {
        HARD5 = 0,
        HARD6,
        HARD7,
        HARD8,
        HARD9,
        HARD10,
        HARD11,
        HARD12,
        HARD13,
        HARD14,
        HARD15,
        HARD16,
        HARD17,
        HARD18,
        HARD19,

        SOFT13,
        SOFT14,
        SOFT15,
        SOFT16,
        SOFT17,
        SOFT18,
        SOFT19,
        SOFT20,
        BLACKJACK,

        DOUBLE1 = 24,
        DOUBLE2,
        DOUBLE3,
        DOUBLE4,
        DOUBLE5,
        DOUBLE6,
        DOUBLE7,
        DOUBLE8,
        DOUBLE9,
        DOUBLE10,
    }
}