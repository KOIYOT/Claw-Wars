using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "StreetVariant", menuName = "Tools/Street Variant Database")]
public class StreetVariant : ScriptableObject
{
    [System.Serializable]
    public class StreetVariant_
    {
        public string Id_;
        public GameObject[] Variants_;
        public float[] Weights_;

        public GameObject GetRandomVariant_()
        {
            if (Variants_ == null || Variants_.Length == 0)
                return null;

            if (Weights_ != null && Weights_.Length == Variants_.Length)
            {
                float TotalWeight_ = 0;
                foreach (float W_ in Weights_)
                    TotalWeight_ += W_;

                float Rand_ = Random.Range(0f, TotalWeight_);
                float Cumulative_ = 0f;

                for (int i = 0; i < Weights_.Length; i++)
                {
                    Cumulative_ += Weights_[i];
                    if (Rand_ <= Cumulative_)
                        return Variants_[i];
                }
            }

            return Variants_[Random.Range(0, Variants_.Length)];
        }
    }

    public List<StreetVariant_> StreetVariants_;

    public GameObject GetVariant_(string id)
    {
        var Match_ = StreetVariants_.FirstOrDefault(v => v.Id_ == id);
        return Match_ != null ? Match_.GetRandomVariant_() : null;
    }
}

// © 2025 KOIYOT. All rights reserved.