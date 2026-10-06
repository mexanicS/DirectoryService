import type { Metadata } from "next";
import { sections } from "@/shared/config/routes";
import { SectionPlaceholder } from "@/widgets/section-placeholder";

export const metadata: Metadata = {
  title: sections.locations.label + " | Directory Service",
};

export default function LocationsPage() {
  return <SectionPlaceholder title={sections.locations.label} />;
}
