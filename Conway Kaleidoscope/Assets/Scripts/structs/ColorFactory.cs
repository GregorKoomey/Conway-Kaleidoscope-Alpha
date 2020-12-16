using UnityEngine;

public struct ColorFactory {

    //Produces color array ranging from [0] == black to [count - 1] == white
        //minimum count is 2
        
    static public Color[] GrayScale(int count) {
        Color[] returnValue;
    
        if (count < 3)
            return GrayScale(); //minimum return is array with black and white only
        
        returnValue = new Color[count];

        float increment = 1f / (count - 1);
        float currentColorValue = 0;

        for (int ind = 0; ind < (count - 1); ind++)
        {
            returnValue[ind] = new Color(currentColorValue, currentColorValue, currentColorValue);
            currentColorValue = currentColorValue + increment;
        }
        
        returnValue[count - 1] = new Color(1f, 1f, 1f);

        return returnValue;
    }

    static public Color[] GrayScaleReversed(int count)
    {
        Color[] returnValue = new Color[count];
        Color[] tempScale = GrayScale(count);

        int newI = count - 1;
        
        for (int i = 0; i < tempScale.Length; i++)
        {
            returnValue[newI] = tempScale[i];
            newI--;
        }
        return returnValue;

    }
    
    static public Color[] GrayScale()
    {
        Color[] returnValue = new Color[2];
        
        returnValue[0] = Color.black;
        returnValue[1] = Color.white;

        return returnValue;

    }


}
