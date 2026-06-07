// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Common.Random
{
  public interface IRandomService
  {
    float Range(
      float inclusiveMin,
      float inclusiveMax,
      bool  nonRepeating = false);

    int Range(
      int inclusiveMin,
      int exclusiveMax,
      bool nonRepeating = false);
  }
}
