"""
This script converts a mesh file (only supports .obj)
for now and converts it into a mesh resource useable in the engine 
"""
import sys
import pathlib
import json

VERT_NAME = "v"
UV_NAME = "vt"
INDICES_NAME = "f"

VALID_VAR = [VERT_NAME, UV_NAME, INDICES_NAME]

def parse_vert(pram: list[str], parsed):
    x = float(pram[0])
    y = float(pram[1])
    z = float(pram[2])
    parsed["Vertices"].append({ "X": x, "Y": y, "Z": z})

def parse_uv(pram: list[str], parsed):
    u = float(pram[0])
    v = float(pram[1])
    parsed["UVs"].append({"X": u, "Y": v})

def parse_indices(pram: list[str], parsed):
    for i, _ in enumerate(pram):
        part = pram[i]
        div = part.split("/")
        mesh_index = int(div[0]) - 1
        parsed["Indices"].append(mesh_index)

def parse_mesh(path: str):
    parsed = {
        "Vertices": [],
        "Indices": [],
        "UVs": [],
        "PrimitiveType": 4 # Triangle
    }
    with open(path, "r") as f:
        for line in f:
            line = line.strip()
            sep = line.split(" ")
            if sep[0] not in VALID_VAR:
                continue
            send = sep[1:]
            match sep[0]:
                case "v":
                    parse_vert(send, parsed)
                case "vt":
                    parse_uv(send, parsed)
                case "f":
                    parse_indices(send, parsed)
    print(f"Parsed {len(parsed["Vertices"])} vertices")
    print(f"Parsed {len(parsed["Indices"])} indices")
    print(f"Parsed {len(parsed["UVs"])} UVs")
    return parsed
                    
# To be noted the script only supports one object in the obj file
if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise Exception("This script need two parameters (import and export files)")
    mesh_path = sys.argv[1]
    export_path = sys.argv[2]
    path = pathlib.Path(mesh_path)
    if path.suffixes[-1] != ".obj":
        raise NameError("This script only accepts .obj")
    parsed = parse_mesh(mesh_path)
    json_p = json.dumps(parsed)
    with open(export_path, "w") as f:
        f.write(json_p)
    print(f"Exported to file \"{export_path}\"")
