#pragma once

#include <tuple>
#include <string>

struct HandCharacteristic;
enum class Fase;

class ISQLiteWrapper  // NOLINT(hicpp-special-member-functions, cppcoreguidelines-special-member-functions)
{
public:
    virtual ~ISQLiteWrapper() = default;
    virtual std::tuple<int, Fase, std::string, bool> GetRule(const HandCharacteristic& handCharacteristic, const Fase& fase, Fase previousFase, int lastBidId) = 0;
    virtual void GetBid(int bidId, int& rank, int& suit) = 0;
    virtual void SetDatabase(const std::string& database) = 0;
};

