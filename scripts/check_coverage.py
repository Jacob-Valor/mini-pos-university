#!/usr/bin/env python3

import argparse
import glob
import sys
import xml.etree.ElementTree as ET


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Validate Cobertura package coverage threshold"
    )
    parser.add_argument(
        "--path", required=True, help="Glob path to coverage.cobertura.xml"
    )
    parser.add_argument(
        "--package", required=True, help="Cobertura package name to validate"
    )
    parser.add_argument(
        "--min-line", required=True, type=float, help="Minimum line coverage percentage"
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    matches = sorted(glob.glob(args.path))

    if not matches:
        print(f"No coverage files matched: {args.path}", file=sys.stderr)
        return 2

    coverage_file = matches[-1]
    root = ET.parse(coverage_file).getroot()
    package_node = root.find(f"./packages/package[@name='{args.package}']")

    if package_node is None:
        print(f"Package '{args.package}' not found in {coverage_file}", file=sys.stderr)
        return 2

    line_rate = float(package_node.attrib.get("line-rate", "0")) * 100
    print(
        f"Coverage for {args.package}: {line_rate:.2f}% "
        f"(required: {args.min_line:.2f}%) from {coverage_file}"
    )

    if line_rate < args.min_line:
        print(
            f"Coverage gate failed for {args.package}: {line_rate:.2f}% < {args.min_line:.2f}%",
            file=sys.stderr,
        )
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
