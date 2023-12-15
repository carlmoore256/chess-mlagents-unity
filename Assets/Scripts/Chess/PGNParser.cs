using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class PGNParser
{
    // Method to parse PGN string and return a PGNData object
    public static PGNData Parse(string pgn)
    {
        var data = new PGNData();
        string[] lines = pgn.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var lineTrimmed = line.Trim();
            if (lineTrimmed.StartsWith("[") && lineTrimmed.EndsWith("]"))
            {
                // Parsing tag pairs
                Parse(lineTrimmed, ref data);
            }
            else
            {
                // Parsing moves
                data.Moves.AddRange(ParseMoves(lineTrimmed));
            }
        }

        return data;
    }

    private static void Parse(string line, ref PGNData data)
    {
        // Remove brackets and split tag pair
        string tagContent = line.Substring(1, line.Length - 2);
        string[] parts = tagContent.Split(new char[] { '"' }, 3);

        string tag = parts[0].Trim();
        string value = parts[1].Trim();

        switch (tag)
        {
            case "Event": data.Event = value; break;
            case "Site": data.Site = value; break;
            case "Date": data.Date = value; break;
            case "Round": data.Round = value; break;
            case "White": data.White = value; break;
            case "Black": data.Black = value; break;
            case "Result": data.Result = value; break;
            case "FEN": data.FEN = value; break;
                // Add additional cases as needed
        }
    }

    private static List<ChessMove> ParseMoves(string line)
    {
        // Implement logic to parse individual move strings from the line
        return new List<ChessMove>(); // Placeholder
    }

    public static PiecePlacement[] GetStartingPositions(string fen)
    {
        var placements = new List<PiecePlacement>();

        string[] parts = fen.Split(' ');
        string[] rows = parts[0].Split('/');

        for (int y = 0; y < rows.Length; y++)
        {
            int x = 0;
            foreach (char c in rows[y])
            {
                if (char.IsDigit(c))
                {
                    // Skip empty squares
                    x += (int)char.GetNumericValue(c);
                }
                else
                {
                    var placement = new PiecePlacement
                    {
                        PieceType = GetPieceType(c),
                        Team = char.IsUpper(c) ? Team.White : Team.Black,
                        SquareId = $"{(char)('a' + x)}{8 - y}"
                    };
                    placements.Add(placement);
                    x++;
                }
            }
        }

        return placements.ToArray();
    }

    private static PieceType GetPieceType(char c)
    {
        switch (char.ToLower(c))
        {
            case 'p': return PieceType.Pawn;
            case 'r': return PieceType.Rook;
            case 'n': return PieceType.Knight;
            case 'b': return PieceType.Bishop;
            case 'q': return PieceType.Queen;
            case 'k': return PieceType.King;
            default: throw new System.ArgumentException($"Invalid piece character: {c}");
        }
    }
}

[System.Serializable]
public class PGNData
{
    public string Event;
    public string Site;
    public string Date;
    public string Round;
    public string White;
    public string Black;
    public string Result;
    public string FEN;
    public List<ChessMove> Moves = new List<ChessMove>();


    public override string ToString()
    {
        return $"Event: {Event}\nSite: {Site}\nDate: {Date}\nRound: {Round}\nWhite: {White}\nBlack: {Black}\nResult: {Result}\nFEN: {FEN}\nMoves: {Moves.Count}";
    }

    public string ToPGNString()
    {
        string pgn = "";

        pgn += $"[Event \"{Event}\"]\n";
        pgn += $"[Site \"{Site}\"]\n";
        pgn += $"[Date \"{Date}\"]\n";
        pgn += $"[Round \"{Round}\"]\n";
        pgn += $"[White \"{White}\"]\n";
        pgn += $"[Black \"{Black}\"]\n";
        pgn += $"[Result \"{Result}\"]\n";
        pgn += $"[FEN \"{FEN}\"]\n";

        int moveNumber = 1;
        foreach (var move in Moves)
        {
            if (moveNumber % 2 == 1)
            {
                pgn += $"{moveNumber}. ";
            }
            pgn += $"{move.FromSquareId}{move.ToSquareId} ";
            moveNumber++;
        }

        return pgn;
    }

    public PiecePlacement[] GetStartingPositions()
    {
        return PGNParser.GetStartingPositions(FEN);
    }
}

public class ChessMove
{
    public string FromSquareId;
    public string ToSquareId;

    // Additional properties like captured piece, promotion, etc., can be added as needed
}