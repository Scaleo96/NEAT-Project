using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rotation { Left, Up, Right, Down }

public class Piece {

    protected Rotation rotation;
    protected Vector2Int position;

    public Piece(Vector2Int position, Rotation rotation = Rotation.Up) {
        this.rotation = rotation;
        this.position = position;
    }

    public Rotation GetRotation() {
        return rotation;
    }

    public void UpdatePosition(Vector2Int position) {
        this.position = position;
    }

    public void Rotate(bool right) {
        rotation = CalculateRotation(rotation, right);
    }

    public void MoveDown() {
        position += Vector2Int.up;
    }

    public void Move(bool right) {
        if(right)
            position += Vector2Int.left;
        else
            position += Vector2Int.right;
    }

    public Vector2Int[] GetOriantation() {
        return Oriantation(rotation, position);
    }

    public Vector2Int[] GetNextOriantation(bool right) {
        Rotation newRot = CalculateRotation(rotation, right);
        return Oriantation(newRot, position);
    }

    public Vector2Int[] GetNextPosition(bool right) {
        if(right)
            return Oriantation(rotation, position + Vector2Int.left);

        return Oriantation(rotation, position + Vector2Int.right);
    }

    protected virtual Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        return new Vector2Int[] { position };
    }

    protected Rotation CalculateRotation(Rotation rot, bool right) {
        Rotation newRot = rot;
        
        //rotate rigth
        if(right) {
            newRot = (Rotation)((int)newRot + 1);

            if(newRot > Rotation.Down)
                newRot = Rotation.Left;

            return newRot;
        }

        //rotate left
        newRot = (Rotation)((int)newRot - 1);

        if(newRot < Rotation.Left)
            newRot = Rotation.Down;

        return newRot;
    }

    public virtual string GetName() {
        return "Piece";
    }

}

public class OPiece : Piece {

    public OPiece(Vector2Int position, Rotation rotation) : base(position, rotation) {}

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];

        // rotation wont change shape 
        // [2](x, y-1) [3](x-1, y-1)
        // [0](x, y)   [1](x-1, y)

        indexes[0] = pos;
        indexes[1] = pos + Vector2Int.left;
        indexes[2] = pos + Vector2Int.down;
        indexes[3] = pos + Vector2Int.down + Vector2Int.left;

        return indexes;
    }

    public override string GetName() {
        return "OPiece";
    }
}

public class IPiece : Piece {

    public IPiece(Vector2Int position, Rotation rotation) : base(position, rotation) { }

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];


        //up             |   //right                                                 |   //down       |  //left  
        // [3](x, y-3)   |   [0](x, y)  [1](x-1, y)   [2](x-2, y)   [3](x-3, y)      |   [0](x, y)    |  [3](x+3, y)    [2](x+2, y)   [1](x+1, y)   [0](x, y)
        // [2](x, y-2)   |                                                           |   [1](x, y+1)  |   
        // [1](x, y-1)   |                                                           |   [2](x, y+2)  |   
        // [0](x, y)     |                                                           |   [3](x, y+3)  |

        switch(rotation) {
            case Rotation.Up:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.down;
                indexes[2] = pos + Vector2Int.down * 2;
                indexes[3] = pos + Vector2Int.down * 3;
                break;
            case Rotation.Right:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.left;
                indexes[2] = pos + Vector2Int.left * 2;
                indexes[3] = pos + Vector2Int.left * 3;
                break;
            case Rotation.Down:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.up;
                indexes[2] = pos + Vector2Int.up * 2;
                indexes[3] = pos + Vector2Int.up * 3;
                break;
            case Rotation.Left:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.right;
                indexes[2] = pos + Vector2Int.right * 2;
                indexes[3] = pos + Vector2Int.right * 3;
                break;
        }

        return indexes;
    }

    public override string GetName() {
        return "IPiece";
    }
}

public class SPiece : Piece {

    public SPiece(Vector2Int position, Rotation rotation) : base(position, rotation) { }

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];


        //up                          |   //flat                                      |   
        // [1](x, y-1)                |                 [0](x, y)   [1](x-1, y)       |   
        // [0](x, y)    [2](x-1, y)   |   [3](x+1, y+1) [2](x, y+1)                   |   
        //              [3](x-1, y+1) |                                               |

        switch(rotation) {
            case Rotation.Up:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.down;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.left + Vector2Int.up;
                break;
            case Rotation.Right:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.left;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.right + Vector2Int.up;
                break;
            case Rotation.Down:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.down;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.left + Vector2Int.up;
                break;
            case Rotation.Left:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.left;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.right + Vector2Int.up;
                break;
        }

        return indexes;
    }

    public override string GetName() {
        return "SPiece";
    }

}

public class ZPiece : Piece {

    public ZPiece(Vector2Int position, Rotation rotation) : base(position, rotation) { }

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];

        //upright                       |  //flat 
        //              [3](x-1, y-1)   |  [1](x+1, y)   [0](x, y) 
        // [0](x, y)    [2](x-1, y)     |                [2](x, y+1)  [3](x-1, y+1)
        // [1](x, y+1)                  |


        switch(rotation) {
            case Rotation.Up:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.up;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.left + Vector2Int.down;
                break;
            case Rotation.Right:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.right;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.left + Vector2Int.up;
                break;
            case Rotation.Down:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.up;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.left + Vector2Int.down;
                break;
            case Rotation.Left:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.right;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.left + Vector2Int.up;
                break;
        }

        return indexes;
    }

    public override string GetName() {
        return "ZPiece";
    }

}

public class LPiece : Piece {

    public LPiece(Vector2Int position, Rotation rotation) : base(position, rotation) { }

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];

        //up                        |   //right                                     |   //down                    |  //left  
        // [3](x, y-2)              |   [0](x, y)    [2](x-1, y)     [3](x-2, y)    |   [1](x+1, y) [0](x, y)   |                             [1](x, y-1)
        // [2](x, y-1)              |   [1](x, y+1)                                 |               [2](x, y+1) |  [3](x+2, y)  [2](x+1, y)   [0](x, y)  
        // [0](x, y)    [1](x-1, y) |                                               |               [3](x, y+2) |   

        switch(rotation) {
            case Rotation.Up:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.left;
                indexes[2] = pos + Vector2Int.down;
                indexes[3] = pos + Vector2Int.down * 2;
                break;
            case Rotation.Right:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.up;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.left * 2;
                break;
            case Rotation.Down:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.right;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.up * 2;
                break;
            case Rotation.Left:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.down;
                indexes[2] = pos + Vector2Int.right;
                indexes[3] = pos + Vector2Int.right * 2;
                break;
        }

        return indexes;
    }

    public override string GetName() {
        return "LPiece";
    }
}

public class JPiece : Piece {

    public JPiece(Vector2Int position, Rotation rotation) : base(position, rotation) { }

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];

        //up                         |  //right                                   |  //down                     |   //left  
        //              [3](x, y-2)  |  [1](x, y-1)                               |  [0](x, y)    [1](x-1, y)   |   [3](x+2, y)  [2](x+1, y)   [0](x, y)
        //              [2](x, y-1)  |  [0](x, y)    [2](x-1, y)     [3](x-2, y)  |  [2](x, y+1)                |                              [1](x, y+1)
        // [1](x+1, y)  [0](x, y)    |                                            |  [3](x, y+2)                |

        switch(rotation) {
            case Rotation.Up:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.right;
                indexes[2] = pos + Vector2Int.down;
                indexes[3] = pos + Vector2Int.down * 2;
                break;
            case Rotation.Right:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.down;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.left * 2;
                break;
            case Rotation.Down:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.left;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.up * 2;
                break;
            case Rotation.Left:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.up;
                indexes[2] = pos + Vector2Int.right;
                indexes[3] = pos + Vector2Int.right * 2;
                break;
        }

        return indexes;
    }

    public override string GetName() {
        return "JPiece";
    }

}

public class TPiece : Piece {

    public TPiece(Vector2Int position, Rotation rotation) : base(position, rotation) { }

    protected override Vector2Int[] Oriantation(Rotation rotation, Vector2Int pos) {
        Vector2Int[] indexes = new Vector2Int[4];

        //up                                    |  //right                   |  //down                                  |   //left  
        //                                      |  [1](x, y-1)               |                                          |               [2](x, y-1)   
        //              [3](x, y-1)             |  [0](x, y)    [3](x-1, y)  |  [2](x+1, y)    [0](x, y)   [1](x-1, y)  |   [3](x+1, y) [0](x, y)                       
        // [1](x+1, y)  [0](x, y)  [2](x-1, y)  |  [2](x, y+1)               |                 [3](x, y+1)              |               [1](x, y+1)

        switch(rotation) {
            case Rotation.Up:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.right;
                indexes[2] = pos + Vector2Int.left;
                indexes[3] = pos + Vector2Int.down;
                break;
            case Rotation.Right:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.down;
                indexes[2] = pos + Vector2Int.up;
                indexes[3] = pos + Vector2Int.left;
                break;
            case Rotation.Down:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.left;
                indexes[2] = pos + Vector2Int.right;
                indexes[3] = pos + Vector2Int.up;
                break;
            case Rotation.Left:
                indexes[0] = pos;
                indexes[1] = pos + Vector2Int.up;
                indexes[2] = pos + Vector2Int.down;
                indexes[3] = pos + Vector2Int.right;
                break;
        }

        return indexes;
    }

    public override string GetName() {
        return "TPiece";
    }

}