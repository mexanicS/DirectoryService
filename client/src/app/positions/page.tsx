import type { Metadata } from "next";
import { sections } from "@/shared/config/routes";
import { SectionPlaceholder } from "@/widgets/section-placeholder";

export const metadata: Metadata = {
  title: sections.positions.label + " | Directory Service",
};

export default function PositionsPage() {
  return <SectionPlaceholder title={sections.positions.label} />;
}
