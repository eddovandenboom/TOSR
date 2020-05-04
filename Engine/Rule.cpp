#include "pch.h"
#include "Rule.h"
#include "Utils.h"
#include <algorithm>
#include <cassert>
#include <iterator>

HandCharacteristic::HandCharacteristic(const std::string& hand)
{
    assert(hand.length() == 16);

    auto suits = Utils::Split<char>(hand, ',');
    std::map<int, size_t> suitLength = {{0, suits[0].length()}, {1, suits[1].length()},
        {2, suits[2].length()}, {3, suits[3].length()}};

    Spades = (int)suitLength[0];
    Hearts = (int)suitLength[1];
    Diamonds = (int)suitLength[2];
    Clubs = (int)suitLength[3];

    std::sort(suits.begin(), suits.end(), [] (const auto& l, const auto& r) {return l.length() > r.length();});
    distribution = std::to_string(suits[0].length()) + std::to_string(suits[1].length()) + 
        std::to_string(suits[2].length())  + std::to_string(suits[3].length());
    isBalanced = distribution == "4333" || distribution == "4432";
    isReverse = CalcuateIsReverse(suitLength, isBalanced);
    shortage = CalculateShortage(suitLength);
    Controls = CalculateControls(hand);
}

bool HandCharacteristic::CalcuateIsReverse(const std::map<int, size_t>& suitLength, bool isBalanced)
{
    std::map<int, size_t> longSuits;
    std::copy_if(suitLength.begin(), suitLength.end(), std::inserter(longSuits, longSuits.begin()), [] (const auto &pair) {return pair.second > 3;});
    return !isBalanced && longSuits.begin()->second == 4;
}

Shortage HandCharacteristic::CalculateShortage(const std::map<int, size_t>& suitLength)
{
    std::map<int, size_t> shortSuits;
    std::copy_if(suitLength.begin(), suitLength.end(), std::inserter(shortSuits, shortSuits.begin()), [] (const auto &pair) {return pair.second < 4;});
    auto minElement = std::min_element(shortSuits.begin(), shortSuits.end(), [] (const auto& l, const auto& r) {return l.second < r.second; });
    // TODO 3 suiters
    // One suiters
    if (shortSuits.size() == 3)
    {
        if (std::count_if(shortSuits.begin(), shortSuits.end(), [&minElement] (const auto& pair) {return pair.second == minElement->second; }) > 1)
        {
            return Shortage::EqualOne;
        }
        if (minElement == shortSuits.begin())
        {
            return Shortage::HighOne;
        }
        if (std::next(minElement) == shortSuits.end())
        {
            return Shortage::LowOne;
        }
        return Shortage::MiddleOne;
    }
    if (shortSuits.size() == 2)
    {
        if (std::count_if(shortSuits.begin(), shortSuits.end(), [&minElement] (const auto& pair) {return pair.second == minElement->second; }) > 1)
        {
            return Shortage::EqualTwo;
        }
        if (minElement == shortSuits.begin())
        {
            return Shortage::HighTwo;
        }
        return Shortage::LowTwo;
    }
    return Shortage::Unknown;
}

int HandCharacteristic::CalculateControls(const std::string& hand)
{
    auto aces = (int)std::count_if(hand.begin(), hand.end(), [] (char c) {return c == 'A';});
    auto kings = (int)std::count_if(hand.begin(), hand.end(), [] (char c) {return c == 'K';});
    return aces * 2 + kings;
}