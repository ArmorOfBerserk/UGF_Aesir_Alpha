using System;

[Serializable]
public class Dialoghi
{
    public Dialogo[] dialoghi;
}

[Serializable]
public class Dialogo
{
    public int id;
    public string testo;
}
