namespace StrideGuard.Domain.Enums;

public enum CategoriaStride
{
    Spoofing = 1,
    Tampering = 2,
    Repudiation = 3,
    InformationDisclosure = 4,
    DenialOfService = 5,
    ElevationOfPrivilege = 6
}

public enum TipoComponente
{
    ApiRest = 1,
    BancoDeDados = 2,
    FrontendSpa = 3,
    MensageriaFila = 4,
    AutenticacaoIdP = 5
}

public enum TrustBoundary
{
    InternetPublica = 1,
    Dmz = 2,
    RedeInterna = 3
}

public enum StatusAmeaca
{
    Aberta = 1,
    EmAnalise = 2,
    Mitigada = 3,
    Aceita = 4
}

public enum NivelRisco
{
    Baixo = 1,
    Medio = 2,
    Alto = 3,
    Critico = 4
}
